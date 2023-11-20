import { Pipe } from "@angular/core";

@Pipe({
  name: 'utcToLocal'
})
export class ConvertUtcToLocal {
  public transform(val: string) {
    if (val) {
      const utcDate = new Date(val); // Create a Date object from the UTC string

      // Get the local timezone offset in minutes
      const localOffset = new Date().getTimezoneOffset();

      // Apply the local offset to the UTC date
      const localDate = new Date(utcDate.getTime() - (localOffset * 60 * 1000));

    return localDate.toDateString()
      + ' '
      + localDate.toTimeString().split(' ')[0];
    }

    return val;
  }
}
