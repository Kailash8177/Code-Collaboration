import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MonacoEditorModule } from 'ngx-monaco-editor-v2';
import { FileService, FileTreeNode, FileContent } from '../../services/file';
import { CollabService } from '../../services/collab';
import { ExecutionService } from '../../services/execution';
import { SnapshotService, Snapshot } from '../../services/snapshot';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-editor',
  imports: [CommonModule, RouterLink, FormsModule, MonacoEditorModule],
  templateUrl: './editor.html',
  styleUrl: './editor.css',
})
export class Editor implements OnInit, OnDestroy {
  projectId: number = 0;
  fileId: number = 0;
  sessionId: string = '';
  isCollabActive: boolean = false;
  
  fileTree: FileTreeNode[] = [];
  
  editorOptions = {theme: 'vs-dark', language: 'javascript'};
  code: string = '// Select a file to start editing...';
  lastSentCode: string = ''; // Used to prevent infinite echo loops
  currentFileName: string = 'No file selected';

  // Execution Output
  executionOutput: string = '';
  isExecuting: boolean = false;
  showOutputPanel: boolean = true;

  // Snapshots
  snapshots: Snapshot[] = [];
  showSnapshotsPanel: boolean = false;

  private collabSub?: Subscription;

  constructor(
    private route: ActivatedRoute,
    private fileService: FileService,
    private collabService: CollabService,
    private executionService: ExecutionService,
    private snapshotService: SnapshotService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.collabService.startConnection();

    this.route.paramMap.subscribe(params => {
      this.projectId = Number(params.get('projectId'));
      const fId = Number(params.get('fileId'));
      
      this.loadProjectTree();
      
      if (fId > 0) {
        this.openFile(fId);
      }

      this.collabSub = this.collabService.codeChange$.subscribe(payload => {
        if (this.isCollabActive && payload && payload.content !== this.code) {
          this.code = payload.content;
          this.lastSentCode = payload.content;
          this.cdr.detectChanges();
        }
      });
    });
  }

  ngOnDestroy() {
    if (this.collabSub) this.collabSub.unsubscribe();
    this.collabService.disconnect();
  }

  onCodeChange(newCode: string) {
    if (this.isCollabActive && this.sessionId && newCode !== this.lastSentCode) {
      this.lastSentCode = newCode;
      this.collabService.sendCodeChange(this.sessionId, newCode);
    }
  }

  loadProjectTree() {
    this.fileService.getProjectTree(this.projectId).subscribe({
      next: (tree) => {
        this.fileTree = tree;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Failed to load file tree', err)
    });
  }

  openFile(id: number) {
    this.fileId = id;
    this.sessionId = `proj_${this.projectId}_file_${id}`;
    
    // Check if user previously explicitly left the session
    const optedOut = sessionStorage.getItem(`collab_optout_${this.projectId}`) === 'true';

    if (!optedOut) {
      // Auto-join collab
      setTimeout(() => {
        this.collabService.joinSession(this.sessionId);
        this.isCollabActive = true;
        this.cdr.detectChanges();
      }, 1000);
    } else {
      this.isCollabActive = false;
    }

    this.fileService.getFileContent(id).subscribe({
      next: (file) => {
        this.code = file.content;
        this.currentFileName = file.name;
        if (file.name.endsWith('.ts') || file.name.endsWith('.js')) this.editorOptions = {...this.editorOptions, language: 'javascript'};
        else if (file.name.endsWith('.py')) this.editorOptions = {...this.editorOptions, language: 'python'};
        else if (file.name.endsWith('.cs')) this.editorOptions = {...this.editorOptions, language: 'csharp'};
        else if (file.name.endsWith('.java')) this.editorOptions = {...this.editorOptions, language: 'java'};
        else if (file.name.endsWith('.html')) this.editorOptions = {...this.editorOptions, language: 'html'};
        this.cdr.detectChanges();
        if (this.showSnapshotsPanel) this.loadSnapshots();
      },
      error: (err) => {
        console.error('Failed to load file', err);
        this.code = '// Error loading file content';
        this.cdr.detectChanges();
      }
    });
  }

  saveFile() {
    if (this.fileId > 0) {
      this.fileService.saveFileContent(this.fileId, this.code).subscribe({
        next: () => {
          console.log('File saved successfully!');
          alert('File saved successfully!');
        },
        error: (err) => {
          console.error('Failed to save file', err);
          alert('Failed to save file.');
        }
      });
    }
  }

  createSnapshot() {
    if (this.fileId > 0) {
      const message = prompt('Enter a commit message for this snapshot:');
      if (message) {
        this.snapshotService.createSnapshot(this.projectId, this.fileId, this.code, message).subscribe({
          next: () => {
            alert('Snapshot created successfully!');
            if (this.showSnapshotsPanel) this.loadSnapshots();
          },
          error: (err) => console.error('Failed to create snapshot', err)
        });
      }
    }
  }

  toggleSnapshotsPanel() {
    this.showSnapshotsPanel = !this.showSnapshotsPanel;
    if (this.showSnapshotsPanel && this.fileId > 0) {
      this.loadSnapshots();
    }
    this.cdr.detectChanges();
  }

  loadSnapshots() {
    this.snapshotService.getSnapshotsForFile(this.fileId).subscribe({
      next: (data) => {
        this.snapshots = data;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Failed to load snapshots', err)
    });
  }

  restoreSnapshot(snapshot: Snapshot) {
    if (confirm(`Are you sure you want to restore the code from snapshot "${snapshot.message}"? This will overwrite your current unsaved code.`)) {
      this.code = snapshot.content;
      this.cdr.detectChanges();
      this.saveFile(); // Optionally auto-save after restore
    }
  }

  toggleCollabSession() {
    if (!this.sessionId) return;
    
    if (this.isCollabActive) {
      // Disconnect
      this.collabService.leaveSession(this.sessionId);
      this.isCollabActive = false;
      sessionStorage.setItem(`collab_optout_${this.projectId}`, 'true');
    } else {
      // Reconnect
      this.collabService.joinSession(this.sessionId);
      this.isCollabActive = true;
      sessionStorage.removeItem(`collab_optout_${this.projectId}`);
    }
    this.cdr.detectChanges();
  }

  runCode() {
    if (!this.code || this.isExecuting) return;
    
    this.isExecuting = true;
    this.showOutputPanel = true;
    this.executionOutput = 'Executing...\n';

    this.executionService.submitCode({
      sourceCode: this.code,
      language: this.editorOptions.language,
      projectId: this.projectId,
      fileId: this.fileId
    }).subscribe({
      next: (res) => {
        this.isExecuting = false;
        if (res.stderr) {
          this.executionOutput = `[ERROR]\n${res.stderr}`;
        } else {
          this.executionOutput = `[OUTPUT]\n${res.stdout}\n\n[TIME]: ${res.executionTimeMs}ms`;
        }
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isExecuting = false;
        this.executionOutput = `[SYSTEM ERROR]\nFailed to execute code. Is the execution service running?`;
        console.error('Execution Failed', err);
        this.cdr.detectChanges();
      }
    });
  }

  toggleOutputPanel() {
    this.showOutputPanel = !this.showOutputPanel;
  }

  createNewFile() {
    const fileName = prompt('Enter new file name (e.g., main.js, index.html):');
    if (fileName) {
      this.fileService.createFile(this.projectId, fileName, fileName, false, this.editorOptions.language).subscribe({
        next: (newFile) => {
          this.loadProjectTree(); // Refresh tree
          this.openFile(newFile.fileId); // Auto-open it
          this.cdr.detectChanges();
        },
        error: (err) => alert('Failed to create file.')
      });
    }
  }
}

