Demonstration:
---------------

-> GUI-Client-Port#8080 , RepoMock-Port#8081 , MotherBuildServer-Port#8082 and TestHarnessMock-Port#8090 will be started at the same time

-> 3 ChildBuilders will be spawned at different ports by MotherBuildServer: ChildBuilder - Port#8083,ChildBuilder-Port#8084 and ChildBuilder-Port#8085

-> I am using 4 TestRequests(.xml) files and 3 Number of Spawned Child Builder Process. (reusing 1 child builder)

-> The 4 BuildRequests are as follows:

  - BuildRequest1.xml -> demo of single test i.e 1 test case of 1 test driver and 2 test files

  - BuildRequest2.xml -> demo of multiple tests i.e 2 test cases

  - BuildRequest3.xml -> demo of handling test failure with exeception handling

  - BuildRequest4.xml -> demo of build fail, hence no TestLog will be created for this

-> For Demonstration, following is the folder structure:

  -> 'StorageClient/' Folder stores BuildRequests created by user using GUI

  -> 'StorageRepoMock/Code' Folder stores C# source code files

  -> 'StorageRepoMock/BuildRequests' Folder stores BuildRequest xml files sent by user through GUI

  -> 'StorageRepoMock/BuildLogs' Folder stores Build Logs xml files

  -> 'StorageRepoMock/TestLogs' Folder stores Test Logs xml files

  -> 'StorageChildBuilder/ChildBuilder#PortNum' Folder stores BuildRequest and TestFiles for each Child Builder which specific unique port number

  -> 'StorageChildBuilder/ChildBuilder#PortNum/MyLibraries' Folder stores libraries(dlls) for each Child Builder which specific unique port number

  -> 'StorageTestHarnessMock/Libraries' Folder stores libraries(dlls) for each TestRequest

 