import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  model: any = {}
  username: string;
  constructor(public accountService: AccountService, private router: Router, private toastr: ToastrService) { }

  ngOnInit(): void {
  }

  login() {
    this.accountService.login(this.model).subscribe(
      { next: (rel:any) => {

        this.username = rel.username;
        this.router.navigateByUrl('/members');

      }
    });
    console.log(this.model);
  }
  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}
