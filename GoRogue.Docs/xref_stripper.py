"""
Module to help us workaround docfx weirdness that prevents us from cross-referencing documentation
in TheSadRogue.Primitives and other non-System dependencies at the moment.
"""

import os
import re

# Regexes for finding SadRogue crefs in docstrings.  Right now we look for things in the
# following namespaces:
#    - SadRogue
#    - Troschuetz
CREF_REGEXES = [r'<\s*?see\s*?cref\s*?=\s*?"(SadRogue\..+?)"\s*?\/\s*?>',
                r'<\s*?see\s*?cref\s*?=\s*?"(Troschuetz\..+?)"\s*?\/\s*?>']

# Directory this script is in so we can use paths relative from it safely
FILE_PATH = os.path.dirname(os.path.realpath(__file__))
# Path to check for CS files
CS_PATH = os.path.join(FILE_PATH, '..', 'GoRogue')


def confirm_continue_prompt() -> bool:
    """Confirms the user is not going to erase work.  Hacks."""
    resp = input("WARNING: This script intends for you to use git commands to reset .cs files " +
                 "after docs generation is complete, in order to undo the script's changes.  " +
                 "Ensure ALL changes to .CS files are committed and pushed first!  " +
                 "Continue? (Y/N): ")
    print()
    return resp in ('y', 'Y')


def convert_to_match(match_obj):
    """Used to replace all matches of a regex with the first group of that regex."""
    return match_obj.group(1)


def main():
    """
    Removes all crefs from external sources that won't resolve from the .cs files so documentation
    can be generated without losing the text.
    """
    if not confirm_continue_prompt():
        return

    for path, _, files in os.walk(CS_PATH):
        for name in files:
            file_path = os.path.join(path, name)

            if file_path.endswith('.cs'):
                with open(file_path, 'rb') as file:
                    file_data = file.read().decode("UTF-8")

                for regex in CREF_REGEXES:
                    file_data = re.sub(regex, convert_to_match, file_data)

                with open(file_path, 'wb') as write:
                    write.write(file_data.encode("UTF-8"))


if __name__ == "__main__":
    main()
