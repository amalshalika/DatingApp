import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { observable, Observable } from 'rxjs';
import { ConfirmDialogComponent } from '../modals/confirm-dialog/confirm-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {

  bsModelRef: BsModalRef;
  constructor(private modalService: BsModalService) { }

  confirm(title = 'Confirmatoin',
    message = 'Are you sure you want to do this?',
    btnOkText = 'Ok', btnCancelText = 'Cancel'): Observable<boolean> {
    const config = {
      initialState:
      {
        title,
        message,
        btnOkText,
        btnCancelText
      }
    };
    this.bsModelRef = this.modalService.show(ConfirmDialogComponent, config);
    return new Observable<boolean>(this.getResult())
  }

  private getResult() {
    return (observable) => {
      const subscription = this.bsModelRef.onHidden.subscribe(() => {
        observable.next(this.bsModelRef.content.result);
        observable.complete();
      });
      return {
         unsubscribe() {
          subscription.unsubscribe();
         }
      }
    }
  }
}
