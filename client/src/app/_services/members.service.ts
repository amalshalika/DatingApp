import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, of } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];

  constructor(private httpClient: HttpClient) { }
  getMembers() {
    if (this.members.length > 0) return of(this.members);
    return this.httpClient.get<Member[]>(this.baseUrl + 'users')
      .pipe(map(members => {
        this.members = members;
        return members;
      }));
  }
  getMember(username: string) {
    const member = this.members.find(x => x.username == username);
    if (member !== undefined) {
      return of(member);
    }
    return this.httpClient.get<Member>(this.baseUrl + 'users/' + username);
  }
  updateMember(member: Member) {
    return this.httpClient.put(this.baseUrl + 'users', member)
      .pipe(map(() => {
        const indexOfMember = this.members.indexOf(member);
        this.members[indexOfMember] = member;
      }));
  }
}
