#!/bin/python

import sys
import re

result = ""
input_filename = ""
output_filename = ""

if len(sys.argv) < 2:
    input_filename = input("Twine file path: ")
else:
    input_filename = sys.argv[1]

if len(sys.argv) < 3:
    output_filename = input("Output yarn script file path: ")
else:
    output_filename = sys.argv[2]

with open(input_filename, "r") as file:
    result = file.read()

    # Delete title and data
    result = re.sub(r":: StoryTitle(.|\n)*?}", r"", result)

    # Delete stylesheet
    result = re.sub(r":: StoryStylesheet(.|\n)*}", r"", result)

    # Delete unused nodes
    result = re.sub(r"::.*\\(.|\n)*?((\n){3,})", r"", result)

    # Delete img tags
    result = re.sub(r"<img.*>", r"", result)

    # Delete leading whitespace
    result = re.sub(r"^([a-zA-Z0-9\s]+)?", r"", result)

    # Delete dialogue options with | in them
    result = re.sub(r"\[\[.*\|.*]]\n", r"", result)
    
    # Node ends
    result = re.sub(r"(\n)(\1{2,})", r"\1===\2", result)

    # Node starts
    result = re.sub(r"::(.*){.*}", r"title:\1\n---", result)

    # Dialogue options
    result = re.sub(r"\[\[(.*)\]\]", r"-> \1\n\t<<jump \1>>", result)
  
    # Remove spaces in node names
    for match in re.finditer(r"title: (.*)", result):
        new_name = match.group(1).replace(" ", "_").replace(":", "_")

        result = result[:match.start(1)] + new_name + result[match.end(1):];

    for match in re.finditer(r"<<jump (.*)>>", result):
        new_name = match.group(1).replace(" ", "_").replace(":", "_")
        result = result[:match.start(1)] + new_name + result[match.end(1):];

    # Remove trailing _ from names
    result = re.sub(r"(title: .*)_", r"\1", result)
    result = re.sub(r"(<<jump .*?)_+(>>)", r"\1\2", result)

    # Set correct start node name
    result = re.sub(r"title: Start_\n", r"title: Start\n", result)



with open(output_filename, "w") as file:
    file.write(result)

