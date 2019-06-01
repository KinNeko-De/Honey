# Honey
package manager based on nuget packages.
It only works on windows.

## Use case
Honey is package manager only designed for one use case:

You have a product that has several parts that should be build and packed seperately.
All parts need more or less the same deployment logic.
You want to deploy as fast as possible.

### Packages
The packages are nuget packages.
You can use nuget to pack your packages.
The deployment logic is based on magic folder names.

### Features
Honey can deploy the following packages out of the box:
* filespackage

But you can add your own deployment logic like the example project 'MyApplicationExample'.

#### Updatemodus
The update modus works best when you preserve 'lastWriteTime' of zip archive entries.
This was added in nuget (i think somewhere around version 4.6.0). if you use older versions of nuget to pack your package the updatemodus can not detect that it is the same file and will replace it. it will worked correctly but the performace is decreased.

Not other methods that using lastWriteTime is supported because zipArchiveEntry only has this property. extracting the entry to get more detailed information is not fast enough. 
