import {Component, OnInit} from '@angular/core';
import {SocialUser} from "@abacritt/angularx-social-login";
import {SecretService} from "../services/secret.service";
import {SecretReturnDto} from "../models/secret-return-dto.model";
import {UserService} from "../services/user.service";
import {ToastrService} from "ngx-toastr";
import {PaginatedResult} from "../models/paginated-result";
import {Clipboard} from "@angular/cdk/clipboard";
import {ConfigLoaderService} from "../services/config-loader.service";
import {CommonConstants} from "../constants/common-constants";

@Component({
  selector: 'app-my-secrets',
  templateUrl: './my-secrets.component.html',
  styleUrl: './my-secrets.component.css'
})

export class MySecretsComponent implements OnInit {
  user: SocialUser | null = null;
  paginatedResult?: PaginatedResult<SecretReturnDto>;
  isLoading = false;
  constructor(private userService: UserService,
    private secretService: SecretService,
    private toastrService: ToastrService,
    private clipboard: Clipboard) {}

  ngOnInit(): void {
    // Check if user state already exist in the user service
    let currentUser = this.userService.currentUserSubject.getValue();
    if (currentUser !== null) {
      this.user = currentUser;
      this.isLoading = true;
      this.secretService.getSecretsSharedByUser(this.user).subscribe({
        next: (paginatedResult) => this.paginatedResult = paginatedResult,
        error: () => {
          this.toastrService.warning("Unable to get secrets, please do logout/login 🚪")
          this.isLoading = false;
        },
        complete: () => this.isLoading = false
        }
      );
    }
  }

  goToPage (goForward: boolean = true) {
    if (this.paginatedResult?.page === undefined) {
      return;
    }
    const nextPage = goForward
      ? this.paginatedResult.page + 1
      : this.paginatedResult.page - 1;
    if (this.user != null) {
      this.secretService.getSecretsSharedByUser(this.user, nextPage).subscribe({
        next: paginatedResult => this.paginatedResult = paginatedResult,
        error: () => this.toastrService.error("Unable to navigate to the next page 🐛."),
      });
    }
  }

  getCopyText(secret: SecretReturnDto) : string {
    if (secret.decryptionKey == null)
    {
      return '⛔️';
    }

    return secret.hasBeenCopied ? '✅' : '✔️'
  }

  copyLinkToTheSecret(secret: SecretReturnDto) {
    let clipContent = ConfigLoaderService.config.siteConfig.siteAddress
      + ConfigLoaderService.config.siteConfig.secret
      + '?id='
      + secret.id;

    if (secret.decryptionKey != null) {
      clipContent += '&accessKey=' + secret.decryptionKey;
    }

    this.clipboard.copy(clipContent);
    secret.linkToTheSecretCopied = true;
  }

  getIndexBasedOnPage (index: number)
  {
    if (!this.paginatedResult?.page
      || !this.paginatedResult?.totalPages
      || this.paginatedResult.page === 1) {
      return index;
    }

    return (this.paginatedResult.page -1) * (this.paginatedResult.pageSize) + index;
  }

  getSecretBody(secret: SecretReturnDto, index: number) {

    if (secret.decryptionKey === null)
    {
      this.toastrService.warning("You cannot copy the secret when the custom access was set. But you can revoke it. 🥸️", '', {
        timeOut: CommonConstants.holdToastWarningForMilliseconds,
        progressBar: true,
        closeButton: true
      });
      return;
    }

    // Handle the case when the secret was loaded before the expiration date, but then it was expired before user clicked copy.
    const availableUntilUtc = new Date(secret.availableUntilUtc);
    const currentDateUtc = this.getNowUTC();

    if (currentDateUtc > availableUntilUtc) {
      this.toastrService.warning("The secret was loaded before the expiration date. However, now it is expired. ⏱️", '', {
        timeOut: CommonConstants.holdToastWarningForMilliseconds,
        progressBar: true,
        closeButton: true
      })
      this.paginatedResult?.data.splice(index, 1);

      return;
    }

    const availableFromUtc = new Date(secret.availableFromUtc);
    if (currentDateUtc < availableFromUtc)
    {
      this.toastrService.warning("Selected secret cannot be viewed right now. Try to access when the date will come (check Available From column). ⏱️", '', {
        timeOut: CommonConstants.holdToastWarningForMilliseconds,
        progressBar: true,
        closeButton: true
      });

      return;
    }

    secret.hasBeenCopied = true;
    this.secretService.getSecret(secret.id, secret.decryptionKey).subscribe({
      next: (result) => {
        this.clipboard.copy(result.secretExtendedDto?.body || '')
        secret.accessAttemptsLeft--;
        if (secret.accessAttemptsLeft === 0) {
          this.toastrService.info("Secret body was copied to the clipboard and the secret was removed because it had the last access attempt.")
          this.paginatedResult?.data.splice(index, 1);
        }
      },
      error: () => this.toastrService.warning(`Unable to copy the secret. 🤖`)
    });
  }

  deleteSecret(secretIndex: number) {
    if (this.paginatedResult) {
      this.secretService.deleteSecret(this.paginatedResult.data[secretIndex].removalKey).subscribe({
        next: () => {
          this.paginatedResult?.data?.splice(secretIndex, 1);
          this.toastrService.success("Secret has been removed ☠️")
        },
        error: (error) => this.toastrService.error("Unable to remove selected secret 😳: " + error)
      });
    }
  }

  private getNowUTC() {
    const now = new Date();
    return new Date(now.getTime() + (now.getTimezoneOffset() * 60000));
  }
}
