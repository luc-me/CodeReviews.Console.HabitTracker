# Habit Tracker with Database
This project allows users to:
*   Add a habit.
*   Insert records for a habit.
*   View all habits and records.
*   Delete a habit.
*   Delete a record.
*   Update a habit.
*   Update a record.

The habits and records are stored in a SQLite database. There are 2 tables, one with the habits, and another with the records.
These tables are connected with a foreign key that reference the record with the habit ID.
To avoid issues with the recursive methods, I use a while loop to the user's input instead.

## Features

*   Implemented parameterized queries to prevent SQL injection.
*   Users can create their own habits.


## Possible improvements:
*   Separate classes into different files.
*   Create reusable methods to reduce repetition.
*   Add unit tests.
