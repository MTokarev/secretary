import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SecretComponent } from './secret/secret.component';
import { HomeComponent } from './home/home.component';
import { AboutComponent } from './about/about.component';
import {MySecretsComponent} from "./my-secrets/my-secrets.component";

const routes: Routes = [
  {
    path: 'secret', component: SecretComponent
  },
  {
    path: 'about', component: AboutComponent
  },
  {
    path: 'my-secrets', component: MySecretsComponent
  },
  { path: '', component: HomeComponent, pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
