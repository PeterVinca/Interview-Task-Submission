
# Interview-Task-Submission

## Structure
Repository contains solution with application, unit test project and zip file with executable.
- Interview-Task-Submission
	- CustomerRequestProcessing.Tests/
	- CustomerRequestProcessing/
		- Core/
		- Files/
		- Services/
	- .gitignore
	- CustomerRequestProcessing_exe.zip
	- README.md (documentation)

Application can be run from provided zip file; and build and run with provided solution in CustomerRequestProcessing

## Application 
Target framework is .Net 10, sdk is .Net 10.0.401

Application uses appsettings.json where are stored values needed for the application, such as path to input files, where error output file should be created (if error found when reading files).  
Application uses:
- CsvHelper
- NodaTime
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Configuration.Binder
- Microsoft.Extensions.Configuration.Json
- Microsoft.Extensions.Configuration.UserSecrets

Application generates one processed request and log files (based on the configuration).

### Functionality

 1. Application creates configuration object from appsettings.json
 2. Creates instance of logger
 3. Loads configuration for input files
 4. Loads data from input files 
 5. Creates output error file if some error where found during the data loading from input files
 6. Load already processed data
 7. Process data according to provided business logic\
 8. Update newly process data

### DateTime Rule
The implementation converts the input timestamp to the Europe/Vienna time zone using NodaTime, performs the SLA hour addition on `LocalDateTime`  preserving local wall-clock semantics across DST transitions and resolves the result back into the Vienna time zone and outputs it in ISO-8601 format.

### Processed request
Processing requests only once is solved using generated processed request file where the information about processed request are stored. Also, processed request file is used for as result file where additional information are stored - if approved or rejected; reason; due date; and follow-up action 

### Errors
Missing a header; missing file → stop  immediately.

Issue with reading a row → add Validation Error for that row and continue.(such data will be missing and that may result in requests being processed as rejected)
