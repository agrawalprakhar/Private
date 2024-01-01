
// auth.guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';
import { Observable, map } from 'rxjs';
import { AccountService } from 'src/app/account/account.service';
import { SharedService } from '../shared.service';
import { User } from '../model/account/user';


@Injectable({
  providedIn: 'root'
})
export class AuthorizationGuard {

  constructor(private accountService: AccountService, private sharedService : SharedService,private router : Router) {}

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean>{

      return this.accountService.user$.pipe(
        map((user : User | null)=>{
          if(user)
          {
            return true;
          }else{
            this.sharedService.showNotification(false,"Resticted Area",'Leave Immediately');
            this.router.navigate(['Account/login'],{queryParams : {returnUrl : state.url}})
            return false;
          }
        })
      ) ;
    }
  }

