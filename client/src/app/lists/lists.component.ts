import { Component, OnInit } from '@angular/core';
import { LikedUserParams } from '../_models/likedUserParams';
import { Member } from '../_models/member';
import { Pagination } from '../_models/pagination';
import { MembersService } from '../_services/members.service';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {
  members: Partial<Member[]>;
  predicate = 'liked';
  pageNumber = 1;
  PageSize = 1;
  pagination: Pagination;
  likedUserParams: LikedUserParams;
  constructor(private memberService: MembersService) { }

  ngOnInit(): void {
    this.loadLikes();
  }
  loadLikes() {
    this.likedUserParams = new LikedUserParams(this.pageNumber, this.PageSize, this.predicate);
    this.memberService.getLikes(this.likedUserParams).subscribe(response => {
      this.members = response.result;
      this.pagination = response.pagination;
    });
  }
  pageChanged($event: any) {
    this.pageNumber = $event.page;
    this.loadLikes();
  }
}
