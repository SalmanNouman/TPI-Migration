# DLX Project Template

This repository contains a template for a DLX project using Unity's **Universal Render Pipeline** and **CORE Framework** packages.

## Setup

In order to set up a DLX project repository from the **DLX Project Template**, this repository can be _forked_ or _cloned_.

### Fork

In Bitbucket, select "Fork this repository" in order to create a repo fork. A benefit of forking the template repository is that updates to the template can be easily pulled in to an existing DLX project in the same manner as a pull request.

### Clone

Instead of forking, the new DLX project repository can be disconnected from the template by cloning (as one typically would with a git repo) then pushing to a new repository remote.



## Git LFS

In either case for copying the project repository, LFS files are not transferred to the new repository, and therefore Git LFS has been disabled in the DLX Project Template. 

To set up Git LFS again, follow the Git LFS steps in Sourcetree (Repository → Git LFS → Initialize Repository)

Then, add a `.gitattributes` file to the root of the project folder with the following contents:

```
*.png filter=lfs diff=lfs merge=lfs -text
*.hdr filter=lfs diff=lfs merge=lfs -text
*.jpg filter=lfs diff=lfs merge=lfs -text
*.jpeg filter=lfs diff=lfs merge=lfs -text
*.tga filter=lfs diff=lfs merge=lfs -text
*.gif filter=lfs diff=lfs merge=lfs -text
*.mp3 filter=lfs diff=lfs merge=lfs -text
*.fbx filter=lfs diff=lfs merge=lfs -text
*.obj filter=lfs diff=lfs merge=lfs -text
*.webm filter=lfs diff=lfs merge=lfs -text
*.mp4 filter=lfs diff=lfs merge=lfs -text
*.m4v filter=lfs diff=lfs merge=lfs -text
```

The `.gitattributes` file can be modified in a text editor or Sourcetree to add or change tracked file types.