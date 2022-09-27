import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from 'src/app/modals/roles-modal/roles-modal.component';
import { User } from 'src/app/_models/user';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {

  users: Partial<User[]> = [];
  bsModalRef?: BsModalRef;
  constructor(private adminService: AdminService, private modalService: BsModalService) { }

  ngOnInit(): void {
    this.getUsersWithRoles();
  }
  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe(users => this.users = users);
  }

  openRolesModal(user: User) {
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        user,
        roles: this.getRolesArray(user)
      }
    };
    this.bsModalRef = this.modalService.show(RolesModalComponent, config);
    this.bsModalRef.content.updateSelectedRoles.subscribe(values => {
      const rolesToUpdate = {
        roles: [...values.filter(el=> el.checked).map(el=>el.name)]
      };
      if(rolesToUpdate) {
        this.adminService.updateUserRoles(user.username, rolesToUpdate.roles).subscribe(() => 
        user.roles = [...rolesToUpdate.roles]);
      }
    });
  }
  private getRolesArray(user: User) {
    const availableRoles = [
      { name: 'Admin', value: 'Admin', checked: false },
      { name: 'Moderator',value: 'Moderator', checked: false },
      { name: 'Member',value: 'Member', checked: false }
    ];
    const activeRoles = user.roles;
    availableRoles.forEach(role => {
      for (let index = 0; index < activeRoles.length; index++) {
        const element = activeRoles[index];
        if (role.name === activeRoles[index]) {
          role.checked = true;
          break;
        }
      }
    });
    return availableRoles;
  }
}
