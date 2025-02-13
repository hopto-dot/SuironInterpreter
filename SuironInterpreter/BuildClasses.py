#!/usr/bin/env python3
import os
import sys
from typing import List

def define_type(writer: str, base_name: str, class_name: str, field_list: str) -> str:
    output = []
    # Start class definition
    output.append(f"        public class {class_name} : {base_name}")
    output.append("        {")

    # Constructor
    output.append(f"            public {class_name}({field_list})")
    output.append("            {")

    # Store parameters in fields
    fields = field_list.split(", ")
    for field in fields:
        name = field.split(" ")[1]
        # Convert parameter name to camelCase for the constructor parameter
        param_name = name[0].lower() + name[1:]
        output.append(f"                {name} = {param_name};")

    output.append("            }")
    output.append("")

    # Fields
    for field in fields:
        type_name, field_name = field.split(" ")
        output.append(f"            public readonly {type_name} {field_name};")

    output.append("        }")
    output.append("")
    return "\n".join(output)

def define_ast(output_dir: str, base_name: str, types: List[str]) -> None:
    path = os.path.join(output_dir, f"{base_name}.cs")
    
    output = []
    # File header
    output.append("using System;")
    output.append("using System.Collections.Generic;")
    output.append("")
    output.append("namespace SuironInterpreter")
    output.append("{")
    output.append(f"    public abstract class {base_name}")
    output.append("    {")

    # AST classes
    for type_def in types:
        class_name = type_def.split(":")[0].strip()
        fields = type_def.split(":")[1].strip()
        class_definition = define_type("", base_name, class_name, fields)
        output.append(class_definition)

    output.append("    }")
    output.append("}")

    # Write the file
    with open(path, "w", encoding="utf-8") as file:
        file.write("\n".join(output))
    
    print(f"Generated {path}")

def main():
    if len(sys.argv) != 2:
        print("Usage: generate_ast.py <output directory>")
        sys.exit(64)

    output_dir = sys.argv[1]

    # Define the AST structure
    define_ast(output_dir, "Expr", [
        "Binary   : Expr Left, Token Operator, Expr Right",
        "Grouping : Expr Expression",
        "Literal  : object Value",
        "Unary    : Token Operator, Expr Right"
    ])

if __name__ == "__main__":
    main()