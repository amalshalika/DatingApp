import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, of } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import {take} from 'rxjs'
import { LikedUserParams } from '../_models/likedUserParams';
import { getPaginatedResult, getPaginationHeader } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];
  memberCache = new Map();
  user: User;
  userParams: UserParams;

  constructor(private httpClient: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UserParams(user);
    });
   }

   getUserParams() {
    return this.userParams;
   }
   setUserParam(userParams:UserParams) {
    this.userParams = userParams;
   }
   resetUserParams() {
    this.userParams = new UserParams(this.user);
    return this.userParams;
   }
  getMembers(userParams: UserParams) {
    var response = this.memberCache.get(Object.values(userParams).join('-'));
    if (response) {
      return of(response);
    }
    let params = getPaginationHeader(userParams.pageNumber, userParams.pageSize);
    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender.toString());
    params = params.append('orderBy', userParams.orderBy.toString());

    return getPaginatedResult<Member[]>(this.baseUrl + 'users', params,this.httpClient)
      .pipe(map(response => {
        this.memberCache.set(Object.values(userParams).join('-'), response);
        return response;
      }));
  }
  getMember(username: string) {
    const member = [...this.memberCache.values()]
    .reduce((arr, elem) => arr.concat(elem.result),[])
    .find((member: Member) =>  member.username === username);

    if(member) {
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
  setMainPhoto(photoId: number) {
    return this.httpClient.put(`${this.baseUrl}users/set-main-photo/${photoId}`, {})
  }
  deletePhoto(photoId: number) {
    return this.httpClient.delete(`${this.baseUrl}users/delete-photo/${photoId}`);
  }
  addLike(username: string) {
    return this.httpClient.post(`${this.baseUrl}likes/${username}`,{});
  }
  getLikes(likeUserParams:LikedUserParams) {
    let params = getPaginationHeader(likeUserParams.pageNumber,likeUserParams.pageSize);
    params = params.append('predicate',likeUserParams.predicate);

    return getPaginatedResult<Partial<Member[]>>(this.baseUrl + 'likes', params,this.httpClient)
    .pipe(map(response => {
      return response;
    }));


    // return this.httpClient.get<Partial<Member[]>>(`${this.baseUrl}likes?predicate=${predicate}`);
  }

}
