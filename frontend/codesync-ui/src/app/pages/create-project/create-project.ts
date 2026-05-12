import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ProjectService } from '../../services/project';

@Component({
  selector: 'app-create-project',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './create-project.html',
  styleUrl: './create-project.css',
})
export class CreateProject {
  name = '';
  description = '';
  language = 'javascript';
  visibility = 'PUBLIC';

  constructor(
    private projectService: ProjectService,
    private router: Router
  ) {}

  onSubmit() {
    this.projectService.createProject(this.name, this.description, this.language, this.visibility)
      .subscribe({
        next: (project) => {
          this.router.navigate(['/editor', project.projectId, 0]);
        },
        error: (err) => {
          console.error('Failed to create project', err);
          alert('Failed to create project.');
        }
      });
  }
}
