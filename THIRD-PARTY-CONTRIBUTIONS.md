# Guidelines for Contributing a 3rd Party Library

### Code of Conduct
  * Anyone contributing to any of the projects in the Steeltoe portfolio, including Steeltoe Initializr, must adhere to our [Contributor Guidelines](https://github.com/SteeltoeOSS/steeltoe#contributing) and [Governance Model](https://github.com/SteeltoeOSS/steeltoe#governance-model).
### Open sourced and available on NuGet.org
  * The library must be open sourced and available on [NuGet.org](https://nuget.org). 
  * The library must be made available with an appropriate OSS license which is deemed compatible with the Apache 2.0 license
### Community
  * There should be a sizeable community, or an established organization, that is actively using and maintaining the library
  * An available support forum and a public issue tracker
### Package Metadata and Documentation
  * The NuGet library should (at a minimum) have these package metadata entries (MSBuild Package Property Names):
     * PackageId
     * PackageVersion
     * Authors
     * Company
     * Title
     * Description/PackageDescription
     * Copyright
     * PackageProjectUrl
     * PackageLicenseFile or PackageLicenseUrl
     * PackageReleaseNotes
     * RepositoryUrl
     * RepositoryBranch
  * For each library on start.steeltoe.io, documentation must be publicly available at the project’s url or on the project’s repository.
  * Documentation should indicate which versions of .NET Core and Steeltoe are supported
### Other Dependencies:
  * The 3rd party library should not bring in any supporting dependencies that aren’t compatible with the Apache 2.0 license
  * All of these supporting dependencies must be available on NuGet.org 
### Test Coverage:
  * The 3rd party library should have proper test coverage that validates it works properly with supported Steeltoe versions
### Steeltoe Engineering Review:
  * The Steeltoe team will periodically review usage statistics (i.e. NuGet downloads, project activity, etc.) and may decide to remove 3rd party libraries from start.steeltoe.io if they are not being used. 
  * The Steeltoe team will periodically check that a 3rd party library is still being supported and that the project is actively releasing updates. If it is determined to no longer or minimally active project, the library will be identified for removal. 
  * Even if these requirements are met, the Steeltoe team may decide that a proposed 3rd party starter does not meet the strategic direction of Steeltoe and the overall portfolio. In this case, it’s still possible to create and share a 3rd party NuGet package but they will not be listed on start.steeltoe.io.
