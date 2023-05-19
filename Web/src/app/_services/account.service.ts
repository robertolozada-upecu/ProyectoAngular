import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  url = 'https://localhost:5002/';

  constructor(private http:HttpClient) { }

  login(model: any)
  {
    return this.http.post(this.url + 'account/login', model);
  }
}
