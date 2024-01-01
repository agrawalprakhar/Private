import { Component, OnInit } from '@angular/core';
import { AccountService } from '../account.service';
import { SharedService } from 'src/app/shared/shared.service';
import { ActivatedRoute, Router } from '@angular/router';
import { take } from 'rxjs';
import { User } from 'src/app/shared/model/account/user';
import { ConfirmEmail } from 'src/app/shared/model/account/confirmEmail';

@Component({
  selector: 'app-confirm-email',
  templateUrl: './confirm-email.component.html',
  styleUrls: ['./confirm-email.component.css']
})
export class ConfirmEmailComponent implements OnInit{
  success  = true;
  
  constructor(private accountService : AccountService,private sharedService : SharedService,private router : Router,
    private activatedRoutes :ActivatedRoute)
    {
      
    }
    ngOnInit(): void {
      this.accountService.user$.pipe(take(1)).subscribe({
        next:(user :User | null ) =>{
        if(user)
        {
          this.router.navigateByUrl('/')
        }else{
          this.activatedRoutes.queryParamMap.subscribe({
            next :(params : any) =>{
              const confirmEmail :ConfirmEmail = {
                token : params.get('token'),
                email : params.get('email')
              }
              
              this.accountService.confirmEmail(confirmEmail).subscribe({
                next : (response :any) =>{
                  this.sharedService.showNotification(true,response.value.title,response.value.message);
                },
                error : error =>{
                  this.success =false;
                this.sharedService.showNotification(false,"failed",error.error);
              }
             })
            }
          })
        }
      }
    })
  }

  resendEmailConfirmationLink() {
    this.router.navigateByUrl('/Account/send-email/resend-email-confirmation-link')
 
  }

  
}
