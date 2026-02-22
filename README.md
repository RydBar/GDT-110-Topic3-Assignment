The project comes with a texture performance monitoring system and an object that can fire non-pooled and pooled bullets.
To test unoptimized textures, make sure the "squareVar#" set of textures places in the "Resources" folder are the 2048x2048 ones without mipmaps or compression.
To test optimized textures, swap the set of "squareVar#" textures in the "Resources" folder with the other optimized set in the "Textures" folder.
To test non-pooled bullets, make sure the "BulletSpawner" object is enabled, and the "Bullet Spawner" script on it is enabled. Disable the other scripts on the object.
To test pooled bullets, make sure the "BulletSpawner" object is enabled, and the "Pooled Bullet Spawner" and "Bullet Pool" scripts are enabled. Disable the "Bullet Spawner" script on the object.
The GitHib of the project is contained in the repository this README is in. Additional link: https://github.com/RydBar/GDT-110-Topic3-Assignment/tree/main
