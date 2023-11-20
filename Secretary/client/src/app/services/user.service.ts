import {Injectable} from '@angular/core';
import {SocialAuthService, SocialUser} from "@abacritt/angularx-social-login";
import {BehaviorSubject} from "rxjs";
import {ToastrService} from "ngx-toastr";
import {CommonConstants} from "../constants/common-constants";
import {ConfigLoaderService} from "./config-loader.service";

@Injectable({
  providedIn: 'root'
})

export class UserService {
  currentUserSubject: BehaviorSubject<SocialUser | null> = new BehaviorSubject<SocialUser | null>(null);
  currentUser?: SocialUser;

  constructor(private authService: SocialAuthService,
              private toastr: ToastrService) {
    this.authService.authState.subscribe({
      next: (user) => {
        this.currentUserSubject.next(user);
      }
    })
  }

  signIn(provideName: string) {
    // Wait until auth service is ready and then do a login.
    // We need to check if popup is blocked in the browser.
    // This might happen if this method is called programatically.
    this.authService.initState.subscribe({
      complete: () => {
        this.authService.signIn(provideName).then((user) => {
          this.currentUser = user;
          this.currentUserSubject.next(user);
          localStorage.setItem(CommonConstants.providerKeyName, provideName);
          this.toastr.success(`Logged in using your '${provideName}' account 🚪`)
        });
      }
    });
  }

  handleAutoLogin() {
    // Check if user state already exist in the user service
    let currentUser = this.currentUserSubject.getValue();
    if (currentUser != null) {
      return;
    }

    const provider = this.getPriviouslyLoggedInProvider();
    if (provider == null) {
      return;
    }

    // Check if popup is blocked and if user was notified before
    if (this.isPopupBlocked() && this.isNotificationRequired()) {
      this.toastr.warning("✅🤖We have detected that you were previously logged in with your "
        + provider
        + " account. Please enable pop-ups and refresh the page if you wish to enable auto-login.",
        '',
        {
          timeOut: CommonConstants.holdToastWarningForMilliseconds,
          closeButton: true,
          progressBar: true,
        }
      );
      const currentDate = new Date();
      localStorage.setItem(CommonConstants.whenUserWasNotifiedAboutPopupKeyName, currentDate.toDateString())

      return;
    }

    this.signIn(provider);
  }

  private isNotificationRequired(): boolean {
    const whenUserWasNotifiedAboutPopup = localStorage.getItem(CommonConstants.whenUserWasNotifiedAboutPopupKeyName);

    if (whenUserWasNotifiedAboutPopup == null) {
      return true;
    }

    // Add days from the config
    const parsedNotificationDate = new Date(whenUserWasNotifiedAboutPopup);
    parsedNotificationDate.setDate(parsedNotificationDate.getDate() + ConfigLoaderService.config.auth.autoLoginPopupSnoozeForDays);

    const currentDate: Date = new Date();

    // If notification date + days from the config is less than current date
    // then notification is required.
    return currentDate >= parsedNotificationDate;
  }

  private isPopupBlocked() {
    const popup = window.open('', '_blank', 'width=1,height=1');

    if (!popup || popup.closed || typeof popup.closed === 'undefined') {
      // Popup is blocked
      return true;
    } else {
      // Popup is not blocked
      popup.close();
      return false;
    }
  }

  signOut() {
    this.authService.signOut(false).then(() => {
      localStorage.removeItem(CommonConstants.providerKeyName);
      this.currentUserSubject.next(null);
      this.toastr.success("Has been logged of 👋🏼")
    });
  }

  getPriviouslyLoggedInProvider(): string | null {
    return localStorage.getItem(CommonConstants.providerKeyName);
  }
}


