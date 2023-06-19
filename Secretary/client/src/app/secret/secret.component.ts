import { HttpStatusCode } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap, Params, Router } from '@angular/router';
import { timer } from 'rxjs';
import { SecretValidationResult } from '../enums/secret-validation-result.enum';
import { ResultSecret } from '../models/result-secret.model';
import { SecretShowDto } from '../models/secret-show-dto.model';
import { SecretService } from '../services/secret.service';
import { ToastrService } from 'ngx-toastr';
import { ConfigLoaderService } from '../services/config-loader.service';
import { Clipboard } from '@angular/cdk/clipboard';

@Component({
  selector: 'app-get-secret',
  templateUrl: './secret.component.html',
  styleUrls: ['./secret.component.css']
})
export class SecretComponent implements OnInit {
  private resultTypes = SecretValidationResult;
  siteAddress: string = ConfigLoaderService.config.siteConfig.siteAddress;
  secretShowDto: SecretShowDto | null = null;
  secretId: string = "";
  removalKey: string = "";
  accessKey: string = "";
  keyRequired: boolean = false;
  toManyRequest: boolean = false;
  retrieveAttemptAllowed: boolean = true;
  secretRemovalFailed: boolean = false;
  keyRetrieved: boolean = false;
  secretCopied: boolean = false;
  secretNotFound: boolean = false;
  showSecret: boolean = false;

  constructor(private secretService: SecretService,
    private route: ActivatedRoute,
    private router: Router,
    private toaster: ToastrService,
    private clipboard: Clipboard) { }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe({
      next: (params: ParamMap) => {
        if (params.has('id')) {
          this.secretId = params.get('id') ?? '';
          this.accessKey = params.get('accessKey') ?? '';
          this.secretService.getSecret(this.secretId, this.accessKey).subscribe({
            next: (result: ResultSecret) => {
              this.secretShowDto = result.secretDto
            },
            error: (error: any) => {
              if (error.error.validationResult === this.resultTypes.PasswordRequired) {
                this.keyRequired = true;
              }
              if (error.status == HttpStatusCode.NotFound) {
                this.secretNotFound = true;
              }
            }
          });
        }

        if (params.has('removalKey')) {
          this.removalKey = params.get('removalKey') ?? '';
          
          this.secretService.deleteSecret(this.removalKey).subscribe({
            next: () => {
              this.secretShowDto = null;
              this.keyRequired = false;
            },
            error: (error: any) => {
              this.secretRemovalFailed = true;
              this.toaster.error(JSON.stringify(error.error));
            }
          });
        }
      }
    });
  }

  getSecret() {
    this.secretShowDto = new SecretShowDto();
    this.secretService.getSecret(this.secretId, this.accessKey).subscribe({
      next: (result: ResultSecret) => {
        this.secretShowDto = result.secretDto;
        this.keyRetrieved = true;
      },
      error: (error: any) => {
        if (error.status == HttpStatusCode.TooManyRequests) {

          const waitInSeconds = ConfigLoaderService.config.siteConfig.retryHttpCallInSeconds;

          this.toaster.info(`Trying to guess the key 🤪? I will let you try again in ${waitInSeconds} seconds...`)
          this.retrieveAttemptAllowed = false;

          // Disable retrieve button for {waitInSeconds} sec
          timer(waitInSeconds * 1000).subscribe({
            next: () => this.retrieveAttemptAllowed = true
          })
        } else if (error.status == HttpStatusCode.NotFound) {
          this.secretNotFound = true;
          this.secretShowDto = null;
          this.keyRequired = false;
        } else if (error.error.validationResult == this.resultTypes.PasswordIncorrect) {
          this.toaster.warning('Access password is incorrect 🔒')
        }
      }
    });
  }

  deleteSecret() {
    this.router.navigate(['/secret'], { 
      queryParams: {
        removalKey: this.secretShowDto?.removalKey
      },
      relativeTo: this.route
    }
    );
    this.secretShowDto = null;
  }

  copyToClipboard() {
    this.clipboard.copy(this.secretShowDto?.body ?? "");
    this.secretCopied = true;
  }

}
