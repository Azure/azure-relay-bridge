import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-order-menu',
  templateUrl: './order-menu.component.html'
})
export class OrderMenuComponent {
  public menuItems: MenuItem[];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<MenuItem[]>(baseUrl + 'menu').subscribe(result => {
      this.menuItems = result;
    }, error => console.error(error));
  }
}

interface MenuItem {
  num: string;
  name: string;
  description: string;
  price: number;
}
