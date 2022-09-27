import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { filter } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  member: Member;
  constructor(private route: ActivatedRoute, public messageService: MessageService,
    private activatedRoute: ActivatedRoute, public presence: PresenceService, 
    private accountService: AccountService, private router: Router) {
      this.accountService.currentUser$.subscribe(user => this.user = user);
      this.router.routeReuseStrategy.shouldReuseRoute = () => false;
     }


  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  messages: Message[] = [];
  @ViewChild('memberTabs', { static: true }) memberTabs: TabsetComponent;
  activeTab: TabDirective;
  user: User;

  ngOnInit(): void {
    this.route.data.subscribe(data => {
      this.member = data.member;
    });

    this.activatedRoute.queryParams.pipe(filter(params => params?.tab)).subscribe(params => {
      params.tab ? this.selectTab(params.tab): this.selectTab(0);
    });
   
    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ];
    this.galleryImages = this.getImages();
  }
  getImages(): NgxGalleryImage[] {
    const imageUrls = [];
    for (const photo of this.member.photos) {
      imageUrls.push({
        small: photo?.url,
        medium: photo?.url,
        big: photo?.url
      });
    }
    return imageUrls;
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading == 'Messages' && this.messages.length === 0) {
      this.messageService.createHubConnection(this.user, this.member.username);
      this.messageService.messageThread$.subscribe(messages => this.messages = messages);
    }
    else {
      this.messageService.stopHubConnection();
    }
  }
  loadMessageTread() {
    this.messageService.getMessageTread(this.member.username)
      .subscribe(response => this.messages = response);
  }
  selectTab(tabId: number) {
    this.memberTabs.tabs[tabId].active = true;
  }  
  
  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }
}
