# Table of contents

- [Secretary overview](#secretary-overview) 🔑
  - [You can log in with a 3rd-party provider](#you-can-log-in-with-a-3rd-party-provider) ✅
    - [Benefits for logged-in users](#benefits-for-logged-in-users)
    - [Providers that are currently supported](#providers-that-are-currently-supported)
    - [Front-End to Back-End communication](#front-end-to-back-end-communication)
    - [What user data the app stores](#what-user-data-the-app-stores)
  - [Is it rocket science?](#is-it-rocket-science) 🚀
  - [What can I do with this service?](#what-can-i-do-with-this-service) ❓
  - [What this service is about?](#what-this-service-is-about) 👍🏼
  - [What this service is NOT about?](#what-this-service-is-not-about) ⛔️
  - [How can I access this service?](#how-can-i-access-this-service) 🚦
  - [Wait a minute, I don't trust you](#wait-a-minute-i-dont-trust-you) 🧐
  - [I don't care that this is open source, it doesn't guarantee your public version is secured](#i-dont-care-that-this-is-open-source-it-doesnt-guarantee-your-public-version-is-secured) 👮🏼‍♂️
  - [It is not ideal](#it-is-not-ideal) 🌍
  - [You promised to easy spin up a container](#you-promised-to-easy-spin-up-a-container) 🚚
    - [SSL](#ssl)
    - [How encryption works](#how-encryption-works) 🔐
  - [Information for nerds](#information-for-nerds) 😎
  - [Found a bug?](#found-a-bug) 🐞
  - [Speak up](#speak-up) 📣
  - [Rather than a thousand words (demo)](#rather-than-a-thousand-words-demo) 💿


# Secretary overview 

The Secretary is a free online service that was created to securely share your confidential data with the recipient.

Many people today send sensitive data using email, social networks, or public IMs. Even though many services do a great job security wise, this doesn't solve the problems of data disposal. A hacked mailbox\account can provide an attacker with confidential information as many users keep received secrets in inboxes.
The secretary never stores unencrypted data and decryption keys for anonymous users.

## You can log in with a 3rd-party provider

While you can use this service anonymously, there is a cool feature that allows you to log in using 3rd-party providers.


### Benefits for logged-in users

If you log in to the app, you have control over all secrets that you've shared. You can interact with these items, such as viewing secret info, copying secrets, secret URLs, or deleting secrets altogether.

`Upcoming feature: Soon you will be able to set the secret recipient from supported 3rd-party providers. All those secrets will be shown once the recipient logs in.`

### Providers that are currently supported

You need to provide configuration for providers that the project supports. Primarily, you need to register the client application with each provider and add configuration for the front and back ends. Please make sure that your site address is added to the redirect URL for your registered app (for local debugging, you can add http://localhost:4200 || https://localhost:4200).

Supported providers:

- Microsoft
- Google
- Facebook
    
Configuration examples =>

Front-End (`config-prd.json`):
```json
"auth": {
    "autoLoginPopupSnoozeForDays": 90,
    "facebook": {
      "clientId": "<ClientId>"
    },
    "google": {
      "clientId": "<ClientId>"
    },
    "microsoft": {
      "clientId": "<ClientId>"
    }
```

`autoLoginPopupSnoozeForDays`` - Required to handle auto-login cases. If the popup is disabled, the user will get a warning toast. If the user ignores or closes this notification, then we won't be annoying the user with the same warning message for 'x' amount of days.

Back-End (`appsettings.json`):
```json
"AuthOptions": {
    "Providers": {
      "Facebook": {
        "BaseUrl": "https://graph.facebook.com/v18.0/",
        "UserProfileEndpoint": "me",
        "TokenEndpoint": "debug_token",
        "ClientId": "<ClientId>",
        "ClientSecret": "<ClientSecret>"
      },
      "Google": {
        "BaseUrl": "https://www.googleapis.com/oauth2/v3/",
        "TokenEndpoint": "tokeninfo"
      },
      "Microsoft": {
        "BaseUrl": "https://graph.microsoft.com/v1.0/",
        "UserProfileEndpoint": "me"
      }
    }
  }
```
Client id and secret are required only for the Facebook provider.

If the client id is missing or empty in the front-end configuration file, then the `Login` button won't show up in the UI, and no auth providers will be configured.

### Front-End to Back-End communication

Once the user logs in and tries to fetch created secrets, the token and the provider name will be attached to the request. The request will then be routed to the backend. The backend will use the issued token and check if the token is valid. For some providers, it will re-use this token to get user information, such as email.

If the token is not valid, then the user will get an Unauthorized HTTP 401 response.


### What user data the app stores

For logged in users the service stores the only `Email Address`. And only on the secrets entity.
No other information is recorded.

## Is it rocket science?

Are you kidding me? Of course not!
I know it is not the only secret sharing service available, but I’ve found this app to be different from others in the range of features it offers.

## What can I do with this service?

Obviously to share sensitive information, so you can control the policies like a boss 😎:

* You always have the option to remove your secret. Once created, You will receive 2 links, one for secret access, and the other one for deleting the secret.
* Optionally, you can allow the user (recipient) to delete the secret.
* Control how many times you want your secret to be accessed. If you set the value to '1' - it
will be deleted immediately after the user opens the secret.
* Add an additional access key. In this case, the user must enter a key next to the link to the secret before receiving the secret. For example, you can send a link using email and a key via SMS.
* You can set when you want your secret to become available. For example, you can share access to your Netflix account tomorrow from 3 PM to 4 PM. The user won't be able to access the secret until 3 PM and after 4 PM your secret will expire and be deleted.
* Feel adventurous? Feel free to integrate with the API and automate your workflow. For instance, if you are working on creating a student accounts, you can call the API to generate a link to the users password and send it to the student’s email.

## What this service is about?

* Share PIN codes.
* Share API keys.
* Share passwords.
* Share phone numbers.
* Share links to the doc (alright-alright, you can generate links in GDoc, Office365, but what if you want that link to be available only at a certain time?).
* Share your geo-location (at a specific time - frame).
* Landlord? Share your car\home locker passcode when needed.
* You want to marry a woman but you are afraid to propose in person. Well, you can say in
person that you have something great to tell at least and share a link that will become available after you leave (purely fictional example 🤵 👰 ).
      
And many other funny and sometimes not funny things.

## What this service is NOT about?

* The secretary was created only for sharing secrets, not storing.
* It is not a 1Password.
* It is not a KeyVault, Hashicorp Vault, etc...
* Seriously, want to have a password manager => use [keychain](https://support.apple.com/lv-lv/guide/mac-help/mchlf375f392/mac) :)

## How can I access this service?

Service is deployed in the public cloud and publicly accessible on [get-secret.com](https://get-secret.com) for free.

## Wait a minute, I don't trust you

This is a valid point, why do you need to trust some guy on the internet who deals with your sensitive information?
Your concerns totally make sense and for that, I decide to make this service open source. This way I hope to establish trust between us.

## I don't care that this is open source, it doesn't guarantee your public version is secured

For you, I have a few options:

* Review the source code, build the service and deploy your own instance with blackjack and hookas.
* If you are not code-friendly but still want to have your own instance, simply spin up a docker container by running a few commands.

## It is not ideal

As everything in our world. I had limited time to work on this service, but I have a lot of ideas on refactoring and bringing new features.
If the service generates curiosity I will work on evolving it further.

## You promised to easy spin up a container

*You can use your docker account, name and tag*
```
git clone https://github.com/MTokarev/secretary.git
cd secretary
docker build -t mtokarevv/secretary1:latest . 
docker run -d -p 8080:8080 --rm mtokarevv/secretary1:latest
```

![docker run](./docs/dockerRun.png)

If you want to put your database outside the container you can mount a volume and change the connection string inside `appsettings.json`.
On the first run if the database does not exist the Secretary will create it for you.

### SSL

While using HTTP on your localhost during development is acceptable, it is strongly recommended that you bind your server in production to HTTPS.
You can put the container behind a proxy server that will handle SSL termination, or check [this guide](https://learn.microsoft.com/en-us/aspnet/core/security/docker-https?view=aspnetcore-6.0) to see how you can pass the certificate to the kestrel server.
I am not a docker expert, but I might spend some time later on making parameters more dynamic.

## Information for nerds

* Backend is written in .NET (8).
* Multi-platform, you can build it to run on Windows, Mac, and Linux.
* Front-end uses Angular framework (17).
* Data layer uses an entity framework, hence it is easy to use any supported servers (MySQL, MSSQL, CosmosDB, etc...), but currently SQLite is used as a storage.
* Kestrel web server is used to serve both the front end and back ends.
* Swagger is used to simplify API integrations.
* Throttling is enabled to protect service from robots and brute-force attacks. After 'x' access attempts (you can configure this behavior in `appsettings.json`) from the same IP, the request will be throttled with HTTP429 (Too many requests).

### How encryption works

**SHA256** is used as a hashing algorithm and **AES** for encryption.
If a user creates an additional key for the secret, then the service uses it to create a symmetric key, otherwise, a random guid is generated for every secret that is later used as a symmetric key.

The secretary doesn't store these keys anywhere. That means the attacker won't be able to decrypt secrets in case of service compromise unless symmetric keys are known.

## Found a bug?

Please report it in github [**issues**](https://github.com/MTokarev/secretary/issues) section.

## Speak up

Have an idea, or would like to discuss something? Please join our [discussions on github](https://github.com/MTokarev/secretary/discussions).

## Rather than a thousand words (demo)
 
If you don't see the `Login` button, then you have probably didn't add auth providers configuration.

![demo](./docs/demo.gif)


