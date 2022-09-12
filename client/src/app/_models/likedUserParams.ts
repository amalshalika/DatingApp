
export class LikedUserParams {
    pageNumber:number;
    pageSize:number;
    predicate:string;
    constructor(pageNumber: number, pageSize: number,predicate:string ) {
        this.pageNumber = pageNumber;
        this.pageSize = pageSize;
        this.predicate = predicate;
    }

}