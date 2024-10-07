import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, Renderer2 } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { Observable, Subject, takeUntil } from 'rxjs';
import { EntityDataService } from 'src/app/angular-app-services/entity-data.service';
import { LayoutService } from 'src/app/angular-app-services/layout.service';
import { Option } from '../dynamic-layout/layout-models';
import { SweetAlertService } from 'src/app/angular-app-services/sweet-alert.service';
import { _camelToSentenceCase, _toSentenceCase } from 'src/app/library/utils';
import { TokenService } from 'src/app/angular-app-services/token.service';
import { compare } from 'fast-json-patch';
import { PatchField } from './patch-field.class';

@Component({
  selector: 'app-template-add',
  templateUrl: './template-add.component.html',
  styleUrl: './template-add.component.scss'
})
export class TemplateAddComponent implements OnInit, OnDestroy {
  @Input() entityName: string = '';
  @Input() id: string = '';
  @Output() saved = new EventEmitter<boolean>();

  public entityDisplayName: string = '';
  public fieldOptions: { [key: string]: Option[]; } = {};
  public form?: FormGroup;
  public layoutData: any[] = [];

  private destroy = new Subject();
  private record: any = null;

  constructor(
    private dialogRef: MatDialogRef<TemplateAddComponent>,
    private entityDataService: EntityDataService,
    private layoutService: LayoutService,
    private renderer: Renderer2,
    private sweetAlertService: SweetAlertService,
    private tokenService: TokenService
  ) {
  }

  ngOnInit(): void {
    this.entityDisplayName = _camelToSentenceCase(this.entityName);
    this.getLayout(this.entityName, this.id ? 'Edit' : 'Add');
  }

  ngOnDestroy(): void {
    this.destroy.next(true);
    this.destroy.complete();
  }

  closeDialog(status: boolean = false): void {
    this.dialogRef.close(status);
  }

  onSubmit(): void {
    const isValid = this.formValidation();
    if (!isValid) return;
    const tenantId = this.tokenService.getTenantId(),
      data = this.unflattenObject(this.form?.value, tenantId ?? '');
    let apiCall: Observable<any>;
    if (this.id) {
      data.Id = this.id;
      const record = { ...this.record },
        patch = (compare(record, data) as PatchField[]).filter(o => !o.path.includes('.') && !o.path.endsWith('_search'));
      apiCall = this.entityDataService.patchRecordById(this.entityName, this.id, patch);
    }
    else {
      apiCall = this.entityDataService.addRecord(this.entityName, data);
    }
    apiCall?.pipe(takeUntil(this.destroy))
      .subscribe({
        next: data => {
          if (data) {
            this.sweetAlertService.showSuccess(`${_toSentenceCase(_camelToSentenceCase(this.entityName))} has been ${this.id ? 'updated' : 'added'}.`);
            this.saved.emit(true);
          }
        }
      });
  }

  private formValidation(): boolean {
    if (!this.form?.valid) {
      for (const hasErrorIn in this.form?.controls) {
        if (this.form.controls[hasErrorIn].errors) {
          this.renderer?.selectRootElement(`#${hasErrorIn}`)?.focus();
          return false;
        }
      }
    }
    return true;
  }

  private getDefaultValue(field: any): any {
    switch (field.dataType.toLowerCase()) {
      case 'numeric':
      case 'guid':
      case 'date':
      case 'datetime':
        return null;
      case 'boolean':
        return false;
      default:
        return '';
    }
  }

  private getLayout(entityName: string, fileName: string): void {
    this.layoutService.getLayout(entityName, fileName)
      ?.pipe(takeUntil(this.destroy))
      .subscribe({
        next: data => {
          this.layoutData = data;
          if (this.id) {
            this.getRecord(this.id);
          } else {
            this.form = new FormGroup({});
            this.initializeForm(data);
            this.getOptions(null);
          }
        }
      });
  }

  private getOptions(data: any): void {
    this.form?.patchValue(data);
  }

  private getRecord(id: string): void {
    const fields = this.entityDataService.getFields(this.layoutData);
    this.entityDataService.getRecordById(this.entityName, id, fields)
      ?.pipe(takeUntil(this.destroy))
      .subscribe({
        next: data => {
          this.record = this.flattenObject(data);
          this.form = new FormGroup({});
          this.initializeForm(this.layoutData, data);
          this.getOptions(this.record);
        }
      });
  }

  private initializeForm(fields: any[], data?: any) {
    // Loop through the fields and add form controls
    fields.forEach(field => {
      if (field.dataType === 'section' || field.dataType === 'groupfield' || field.dataType === 'tab') {
        this.initializeForm(field.fields, data);
      } else {
        const defaultValue = this.getDefaultValue(field),
          validators: any[] = [];
        if (
          (typeof field.required === 'boolean' && field.required) ||
          (typeof field.required === 'string' && field.required.toLowerCase() === 'true')
        ) {
          validators.push(Validators.required);
          field.required = true;
        } else {
          field.required = false;
        }

        if (field.dataType.toLowerCase() === 'numeric') {
          if (field.scale) {
            const scale = parseInt(field.scale),
              regEx = RegExp('^-?[0-9]+(\\.[0-9]{0,' + scale + '}){0,1}$');
            validators.push(Validators.pattern(regEx));
          } else {
            validators.push(Validators.pattern(/^-?\d+$/));
          }
        }

        if ((field.dataType.toLowerCase() === 'datetime' || field.dataType.toLowerCase() === 'date') && data?.[field.fieldName]) {
          const date = Date.parse(data[field.fieldName] + 'Z'),
            localDate = isNaN(date) ? date : new Date(data[field.fieldName] + 'Z');
          data[field.fieldName] = localDate;
        }

        this.form?.addControl(field.fieldName, new FormControl(defaultValue, validators));
        if (field.dataType?.toLowerCase() === 'guid') {
          this.form?.addControl(field.fieldName.replaceAll('.', '_') + '_search', new FormControl(''));
          if (!this.fieldOptions[field.fieldName]) {
            this.fieldOptions[field.fieldName] = [];
          }
        }
      }
    });
  }

  private flattenObject(record: any): any {
    const result: any = { ...record };

    for (const key in record) {
      if (Object.prototype.hasOwnProperty.call(record, key) && typeof record[key] === 'object' && record[key] !== null) {
        const nestedObj = record[key];

        for (const nestedKey in nestedObj) {
          if (Object.prototype.hasOwnProperty.call(nestedObj, nestedKey)) {
            const newKey = `${key}.${nestedKey}`;
            result[newKey] = nestedObj[nestedKey];
          }
        }
      }
    }

    return result;
  }

  private unflattenObject(obj: any, tenantId: string): any {
    const result: any = {};

    for (const key in obj) {
      if (Object.prototype.hasOwnProperty.call(obj, key)) {
        const keys = key.split('.');
        if (keys.length > 1 && !result[keys[0]]) {
          const existingIdKey = keys[0].split('_')[0],
            recordId = this.id ? this.record[existingIdKey] : crypto.randomUUID();
          result[keys[0]] = {
            'TenantId': tenantId,
            'Id': recordId
          };
        }
        keys.reduce((acc, k, index) => {
          if (index === keys.length - 1) {
            acc[k] = obj[key];
          } else {
            if (!acc[k]) {
              acc[k] = {};
            }
          }
          return acc[k];
        }, result);
      }
    }

    return result;
  }
}
