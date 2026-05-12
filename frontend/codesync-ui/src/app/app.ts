import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from './components/navbar/navbar'; // <-- Add this import

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar], // <-- Add Navbar here
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('codesync-ui');
}
