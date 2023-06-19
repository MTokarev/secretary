import { AbstractControl, ValidationErrors, ValidatorFn } from "@angular/forms";

export function dateNotInThePast(control: AbstractControl): {[s: string]: boolean} | null {
  const date = Date.parse(control.value);

  // Minus 10 minutes, otherwise the date will be incorrect right away
  const currentDate = new Date();
  currentDate.setMinutes(currentDate.getMinutes() - 10);
  
  if (date < currentDate.getTime()) {
    return {'Date must not be in the past': true};
  }
  return null;
}

export function firstDateMustBeGreaterThanSecond(dateToCompare: AbstractControl): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const dateToCompareParsed = Date.parse(dateToCompare.value);
    const dateFromFormParsed = Date.parse(control.value);
    if(dateToCompareParsed > dateFromFormParsed ) {
      return {"End date cannot be greater than start date.": true}
    } 
    return null;
  }
}
