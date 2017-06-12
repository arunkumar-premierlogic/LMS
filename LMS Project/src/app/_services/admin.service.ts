import { Injectable } from '@angular/core';
import { Http, Headers, RequestOptions, Response } from '@angular/http';

import { User,types,applyLeaveModel,approvedLeaveModel } from '../_models/index';
import { ConfigService } from '../config/apiconfig';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class AdminService{

    _baseUrl: string = '';
    constructor(private http: Http,private _webapi:ConfigService) { 
        this._baseUrl = _webapi.getApiURI();
    }

    LeaveRequests() {   
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({ headers: headers });   
        return this.http.get(this._baseUrl +'/admin/LeaveRequests',options)
            .map((response: Response) => {
                let leaverequests = response.json();
              // console.log(leaverequests);
                return leaverequests;
            });
    }   

    // private helper methods
    private jwt() {
        // create authorization header with jwt token
        let currentUser = JSON.parse(localStorage.getItem('currentUser'));
        if (currentUser && currentUser.token) {
            let headers = new Headers({ 'Authorization': 'Bearer ' + currentUser.token });
            return new RequestOptions({ headers: headers });
        }
    }
}