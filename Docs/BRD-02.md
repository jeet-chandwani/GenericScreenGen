# This file has my requirements for layout policy changes.

1. Change per-line layout policy so that field label and corresponding control are displayed on same line.

2. The current UI of home page is taking too much space for Title and selection of screens and showing screen files. Lets change as follows:
    a. Title should be on top and centered - make it 1 liner.
    b. Below title, there should be a horizontal line.
    c. Below horizontal line, there should be a list of all screen files avialble with status of valid/invalid. This should be in 2 columns - file name and status. The file name should be left aligned and status should be right aligned.
    d. The list of screen files should be scrollable if there are more than 5 files. The horizontal line should be fixed and not scroll with the list of screen files.
    e. The layout should be responsive and adjust to different screen sizes.
    f. The layout should be visually appealing and easy to read.
    g. Each screen line should be clickable and should navigate to the corresponding screen when clicked.
    h. The status of each screen file should be color coded - green for valid and red for invalid.
    i. Each screen displayed from screen config file should have a button to get back to home page.

3. All layout policies should support dynamic resizing as screen or window size changes.
 
4. Add field types
    a. text with type-info = {min-chars=0;max-chars=10;lines=1;} 
    b. date -- should allow user to choose date from UI control
    c. date-time -- should allow user to choose date and time from UI control
    d. lookup with type-info = {semi-colon separated list of values}

5. On click of Refresh Screens button on home page, we should re-load and re-validate all screen config files and update the status of each screen file in the UI accordingly. The validation should check for required fields, field types, and any other constraints specified in the screen config schema.

6. For exisitng screen cofnig files, we should support both old and new layout policies. We can add a new field in screen config file to specify the layout policy to be used for that screen. If the field is not specified, we can default to old layout policy.

7. For exisiting screen config files, update as follows:
    a. For fields with type = text, we should add type-info with default values as mentioned in point 4a.
    b. For fields with type = date or date-time, we should add the corresponding UI controls to allow user to select date and time.
    c. For fields with type = lookup, we should add the corresponding UI controls to allow user to select from the list of values specified in type-info.

8. Add a new layout policy = flow. The fields should be displayed until they wrap to next line. The field label and corresponding control should be on same line. The fields should be left aligned and there should be a small gap between fields. The layout should adjust dynamically as screen size changes.

