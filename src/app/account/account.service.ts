import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Register } from '../shared/model/account/register';
import { environment } from 'src/environments/environment.development';
import { Login } from '../shared/model/account/login';
import { User } from '../shared/model/account/user';
import { ReplaySubject, map, of } from 'rxjs';
import { Router } from '@angular/router';
import { ConfirmEmail } from '../shared/model/account/confirmEmail';
import { ResetPassword } from '../shared/model/account/reset-password';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private userSource = new ReplaySubject<User | null>(1);
  user$ = this.userSource.asObservable();

  constructor(private http: HttpClient, private router: Router) { }
  
  refreshUser(jwt: string | null) {
    if (jwt === null) {
      this.userSource.next(null);
      return of(undefined);
    }
    
    let headers = new HttpHeaders();
    headers = headers.set('Authorization', 'Bearer ' + jwt);
    
    return this.http.get<User>(`${environment.appurl}/Account/refresh-user-token`, { headers }).pipe(
      map((user: User) => {
        if (user) {
          this.setUser(user);
        }
      })
      )
    }
    
    register(model: Register) {
      return this.http.post(`${environment.appurl}/Account/register`, model);
    }
    
    confirmEmail(model : ConfirmEmail)
    {
      return this.http.put(`${environment.appurl}/Account/confirm-email`,model)
    }
    
    resendEmailConfirmationLink(email: string) {
      return this.http.post(`${environment.appurl}/Account/resend-email-confirmation-link/${email}`, {});
    }
    
    forgotUsernameOrPassword(email: string) {
      return this.http.post(`${environment.appurl}/Account/forgot-username-or-password/${email}`, {});
    }
    resetPassword(model: ResetPassword) {
      return this.http.put(`${environment.appurl}/Account/reset-password`,model);
    }
    
    
    login(model: Login) {
    return this.http.post<User>(`${environment.appurl}/Account/login`, model).pipe(
      map((user: User) => {
        if (user) {
          this.setUser(user);
        }
      })
      );
    }

  logout() {
    localStorage.removeItem(environment.userKey);
    this.userSource.next(null);
    this.router.navigateByUrl('/');

  }
  getJWT() {
    const key = localStorage.getItem(environment.userKey);
    if (key) {
      const user: User = JSON.parse(key)
      return user.jwt;
    } else {
      return null;
    }
  }


  private setUser(user: User) {
    localStorage.setItem(environment.userKey, JSON.stringify(user));
    this.userSource.next(user);
  }
}
