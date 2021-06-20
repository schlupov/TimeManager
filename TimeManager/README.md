#TimeManager
The application allows you to manage working hours, breaks and vacations.

##Author
Silvie Chlupová\
437213@mail.muni.cz

##Login
To log in to the application, you must first register.
After launching the application, you will be offered the option to log in or register, so select register.
Fill in the name, email and password.
After registration, you will be logged in to the application with the data filled in and you can then select
one of the 17 options that the application allows.

##Possible actions
I will describe the most fundamental options.

Option 4 (Create a new time record): record means a work record, ie work done that day.
This can be a bugzilla, issue, documentation or release.
There may be more than one such record on a given day. You can work from 8 to 22.
You can also add a voluntary comment.

Option 5 (Read records by day): you can also read records by date, so if you filled in the date 4/5/2021 in option 4,
then you can view this record with option 5.

Option 9 (Add a break): You can also add a break that is limited only by working hours and also by adding a break only on the day you worked.

Option 12 (Add a vacation): It is only possible to add vacation on a day when there is no work record.

Option 14 (Show your work by month): Shows all work records, all breaks and all vacations in a given month of the year.

Option 15 (Read work time from file by month): Retrieves the contents of the log from irc chat, where all communication in the company takes place.
The chat log has a form that TimeManager knows and can read the beginning and end of working hours accordingly.
The path to the log is entered interactively during program run, or it can be set in the config.json file.

##config.json

The file is used to speed up the login and read the chat log. The file can save the email, password, name and path to the chat log.
Use of this file is optional.