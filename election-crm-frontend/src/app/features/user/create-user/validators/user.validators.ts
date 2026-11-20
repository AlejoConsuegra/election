import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export class UserValidators {
  /**
   * Valida que la contraseña tenga una longitud mínima (OWASP ASVS V2.1.1: 12 caracteres)
   */
  static passwordMinLength(min: number): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null; // Dejamos que 'required' maneje los vacíos
      return control.value.length >= min
        ? null
        : { passwordMinLength: { requiredLength: min, actualLength: control.value.length } };
    };
  }

  /**
   * Valida que la cédula solo contenga números y tenga una longitud razonable (6 a 15 dígitos)
   */
  static cedulaFormat(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      const valid = /^[0-9]{6,15}$/.test(control.value);
      return valid ? null : { cedulaFormat: true };
    };
  }

  /**
   * Valida que el teléfono solo contenga números
   */
  static phoneFormat(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      const valid = /^[0-9]{7,15}$/.test(control.value);
      return valid ? null : { phoneFormat: true };
    };
  }
}
