import {formatDate} from '@angular/common';
import {AfterViewInit, Component, ElementRef, Inject, LOCALE_ID, OnDestroy, OnInit, Renderer2} from '@angular/core';
import {FormControl, FormGroup, Validators} from '@angular/forms';
import {SecretCreateDto} from '../models/secret-create-dto.model';
import {SecretReturnDto} from '../models/secret-return-dto.model';
import {ConfigLoaderService} from '../services/config-loader.service';
import {SecretService} from '../services/secret.service';
import {Clipboard} from '@angular/cdk/clipboard';
import {dateNotInThePast, firstDateMustBeGreaterThanSecond} from '../utils/validators/date.validator';
import {Subscription} from 'rxjs';
import {ToastrService} from 'ngx-toastr';
import {
  FacebookLoginProvider,
  GoogleLoginProvider, MicrosoftLoginProvider,
  SocialUser
} from "@abacritt/angularx-social-login";
import {UserService} from "../services/user.service";
import {CommonConstants} from "../constants/common-constants";

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, OnDestroy, AfterViewInit {
  protected readonly GoogleLoginProvider = GoogleLoginProvider;
  protected readonly FacebookLoginProvider = FacebookLoginProvider;
  private defaultValidInDays: number = 3;
  secretAddress: string = ConfigLoaderService.config.siteConfig.siteAddress + ConfigLoaderService.config.siteConfig.secret;
  swaggerAddress: string = ConfigLoaderService.config.urls.base + ConfigLoaderService.config.urls.swagger;
  secretFormStartDateSub: Subscription = new Subscription();
  secretFormKeyRequiredSub: Subscription = new Subscription();
  secretFormAccessCountRequiredSub: Subscription = new Subscription();
  secretForm: FormGroup = new FormGroup({});
  secretCreateDto?: SecretCreateDto;
  secretReturnDto: SecretReturnDto = new SecretReturnDto();
  showForm: boolean = true;
  showResult: boolean = false;
  secretLinkCopied: boolean = false;
  removalLinkCopied: boolean = false;
  isSecretBodyInvalid: boolean = false;
  isGoogleProviderEnabled: boolean = ConfigLoaderService.config.auth?.google?.clientId?.length > 0;
  isFacebookProviderEnabled: boolean = ConfigLoaderService.config.auth?.facebook?.clientId?.length > 0;
  isMicrosoftProviderEnabled: boolean = ConfigLoaderService.config.auth?.microsoft?.clientId?.length > 0;
  user: SocialUser | null = null;
  isDropdownOpen: boolean = false;

  constructor(@Inject(LOCALE_ID) public locale: string,
    private secretService: SecretService,
    private toastr: ToastrService,
    private clipboard: Clipboard,
    private userService: UserService,
    private renderer: Renderer2,
    private element: ElementRef) {}

  ngAfterViewInit(): void {
      this.setFocusOnSecretBody();
    }

  ngOnDestroy(): void {
    this.secretFormStartDateSub.unsubscribe();
    this.secretFormKeyRequiredSub.unsubscribe();
    this.secretFormAccessCountRequiredSub.unsubscribe();
  }

  ngOnInit(): void {
    // Subscribe to the user state
    this.userService.currentUserSubject.subscribe((user) => {
      this.user = user;
      if (user != null) {
        this.isDropdownOpen = false;
      }
    });

    // Relogin user if login provider is found in the localStorage
    this.userService.handleAutoLogin();

    this.secretForm.addControl('startDate', new FormControl('', [Validators.required, dateNotInThePast]));
    this.secretForm.addControl('endDate', new FormControl('', [Validators.required, dateNotInThePast, firstDateMustBeGreaterThanSecond(this.secretForm.controls['startDate'])]));
    this.secretForm.addControl('requestText', new FormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(2048)]));
    this.secretForm.addControl('accessKeyRequired', new FormControl(false));
    this.secretForm.addControl('accessKey', new FormControl('', [Validators.minLength(1)]));
    this.secretForm.addControl('accessCountRequired', new FormControl(false));
    this.secretForm.addControl('accessCount', new FormControl(1, [Validators.min(0)]));
    this.secretForm.addControl('selfRemovalAllowed', new FormControl(false));

    this.secretFormStartDateSub = this.secretForm.controls['startDate'].valueChanges.subscribe({
      next: () => {
        const endDateControl = this.secretForm.controls['endDate'];
        if (endDateControl.value) {
          endDateControl.updateValueAndValidity();
          endDateControl.markAsTouched();
        }
      }
    });

    this.secretFormKeyRequiredSub = this.secretForm.controls['accessKeyRequired'].valueChanges.subscribe({
      next: () => {
        const condition = this.secretForm.controls['accessKeyRequired'];

        // Timeout is required to avoid issue with NG0100 (item updated after it was initialized)
        // More details could be found here: https://angular.io/errors/NG0100
        setTimeout(() => {
          if (condition.value) {
            this.secretForm.controls['accessKey'].addValidators(Validators.required);
          } else {
            this.secretForm.controls['accessKey'].removeValidators(Validators.required);
          }
          this.secretForm.controls['accessKey'].updateValueAndValidity();
        }, 0);
      }
    });

    this.secretFormAccessCountRequiredSub = this.secretForm.controls['accessCountRequired'].valueChanges.subscribe({
      next: () => {
        const condition = this.secretForm.controls['accessCountRequired'];

        // Timeout is required to avoid issue with NG0100 (item updated after it was initialized)
        // More details could be found here: https://angular.io/errors/NG0100
        setTimeout(() => {
          if (condition.value) {
            this.secretForm.controls['accessCount'].addValidators(Validators.required);
          } else {
            this.secretForm.controls['accessCount'].removeValidators(Validators.required);
          }
          this.secretForm.controls['accessCount'].updateValueAndValidity();
        }, 0);
      }
    });

    this.secretForm.controls['requestText'].valueChanges.subscribe({
      next: (value: any) => {
        if (this.secretForm.controls['requestText'].touched
          && this.secretForm.controls['requestText'].value?.length === 0) {
          this.isSecretBodyInvalid = true;
        }

        if(this.secretForm.controls['requestText'].value?.length > 0) {
          this.isSecretBodyInvalid = false;
        }
      }
    });

    this.initSecretForm();
  }

  setFocusOnSecretBody() {
    const elementToFocus = this.element.nativeElement.querySelector("#secretBody");
    if (elementToFocus) {
      this.renderer.selectRootElement(elementToFocus).focus();
    }
  }

  toggleDropdown () {
    this.isDropdownOpen = ! this.isDropdownOpen;
  }

  login (provider: string) {
    if (this.user === null) {
      this.isDropdownOpen = !this.isDropdownOpen;
      this.userService.signIn(provider);
    }
  }

  logOut () {
    this.userService.signOut();
  }

  createAnotherSecret() {
    this.showResult = false;
    this.showForm = true;

    this.secretLinkCopied = false;
    this.removalLinkCopied = false;

    this.initSecretForm();
  }

  initSecretForm() {
    const now = new Date();

    this.secretForm.reset();
    this.secretForm.patchValue({'startDate': formatDate(now, 'yyyy-MM-ddTHH:mm', this.locale)});
    this.secretForm.patchValue({'endDate': formatDate(new Date().setDate(now.getDate() + this.defaultValidInDays), 'yyyy-MM-ddTHH:mm', this.locale)});
    this.secretForm.patchValue({'selfRemovalAllowed': false});
    this.secretForm.patchValue({'accessCount': 1});
    this.secretForm.patchValue({'requestText': ''});
  }

  handleSubmitFormByKeyboard(event: KeyboardEvent) {
    if (!(event.ctrlKey && event.key === 'Enter'))
    {
      return;
    }

    if (this.secretForm.controls['requestText'].value?.length == 0) {
      this.isSecretBodyInvalid = true;
      return;
    }
    this.onSubmit();
  }

  onSubmit() {
    this.secretCreateDto = new SecretCreateDto();
    this.secretCreateDto.body = this.secretForm.get('requestText')?.value;
    this.secretCreateDto.availableFromUtc = new Date(this.secretForm.get('startDate')?.value).toISOString();
    this.secretCreateDto.availableUntilUtc = new Date(this.secretForm.get('endDate')?.value).toISOString();
    this.secretCreateDto.selfRemovalAllowed = this.secretForm.get('selfRemovalAllowed')?.value;
    this.secretCreateDto.sharedByEmail = this.user?.email ?? null;

    if (this.secretForm.get('accessCountRequired')?.value) {
      this.secretCreateDto.accessAttemptsLeft = this.secretForm.get('accessCount')?.value;
    }

    if (!this.accessKeyEmpty) {
      this.secretCreateDto.accessPassword = this.secretForm.controls['accessKey'].value;
    }

    this.secretService.createSecret(this.secretCreateDto).subscribe({
      next: (result: SecretReturnDto) => {
        this.secretReturnDto = result;
        this.showForm = false;
        this.showResult = true;
      },
      error: (error: any) => this.toastr.error(JSON.stringify(error.error))
    });
  }

  copyToClipboard(isSecretId: boolean) {
    if (isSecretId) {
      let clipContent = this.secretAddress + '?id=' + this.secretReturnDto.id;
      if (this.accessKeyEmpty){
       clipContent += '&accessKey=' + this.secretReturnDto.accessPassword;
      }

      this.clipboard.copy(clipContent);
      this.secretLinkCopied = true;
      setTimeout(() => {
        this.secretLinkCopied = false;
      }, CommonConstants.changeHtmlImageBackInMilliseconds)
    } else {
      this.clipboard.copy(this.secretAddress + '?removalKey=' + this.secretReturnDto.removalKey);
      this.removalLinkCopied = true;
      setTimeout(() => {
        this.removalLinkCopied = false;
      }, CommonConstants.changeHtmlImageBackInMilliseconds)
    }
  }

  get accessKeyEmpty(): boolean {
    return !this.secretForm.controls['accessKeyRequired'].value
      || this.secretForm.controls['accessKey'].value?.length === 0;
  }

  protected readonly MicrosoftLoginProvider = MicrosoftLoginProvider;
}
