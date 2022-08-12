import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'The Dating App';
  users: any;
  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.getUsers();
  }

  private getUsers() {
    this.http.get('https://localhost:5001/api/users')
      .subscribe({
          next: (rel) => this.users = rel, 
          error: (error) => console.log(error)
        });
  }
}
