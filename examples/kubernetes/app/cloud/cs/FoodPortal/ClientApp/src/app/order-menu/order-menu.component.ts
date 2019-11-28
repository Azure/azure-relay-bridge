import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-order-menu',
  templateUrl: './order-menu.component.html'
})
export class OrderMenuComponent {
  public menuItems: MenuItem[];
  public restaurants: Restaurant[];
  httpClient: HttpClient;
  baseUrl: string;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {

    this.httpClient = http;
    this.baseUrl = baseUrl;
    http.get<Restaurant[]>(baseUrl + 'restaurant').subscribe( result => {
       this.restaurants = result;
    }, error => console.error(error))
  }

  restaurantSelected(event) {
    this.httpClient.get<MenuItem[]>(this.baseUrl + 'menu').subscribe(result => {
      console.log(result);
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

interface Restaurant {
  id: string;
  name: string;
}
