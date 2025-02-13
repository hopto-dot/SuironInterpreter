#!/usr/bin/env python3
import os
import sys
from typing import List

def to_pascal_case(name: str) -> str:
    return name.strip().title()

def to_camel_case(name: str) -> str:
    name = name.strip()
    return name[0].lower() + name[1:]

def escape_keyword(name: str) -> str:
    # List of C# keywords that need escaping
    keywords = ["operator"]
    if name.lower() in keywords:
        return "@" + name
    return name

def define_type(writer: str, base_name: str, class_name: str, field_list: str) -> str:
    output = []
    # Class name should be PascalCase
    class_name = to_pascal_case(class_name)
    
    # Start class definition
    output.append(f"        public class {class_name} : {base_name}")
    output.append("        {")

    # Process fields for constructor
    fields = field_list.split(", ")
    constructor_params = []
    for field in fields:
        type_name, field_name = field.split(" ")
        # Keep the parameter names in camelCase and escape keywords
        param_name = escape_keyword(to_camel_case(field_name))
        # Keep the field names in PascalCase
        field_name = to_pascal_case(field_name)
        constructor_params.append(f"{type_name} {param_name}")

    # Constructor
    output.append(f"            public {class_name}({', '.join(constructor_params)})")
    output.append("            {")

    # Store parameters in fields
    for field in fields:
        type_name, field_name = field.split(" ")
        param_name = escape_keyword(to_camel_case(field_name))
        field_name = to_pascal_case(field_name)
        output.append(f"                {field_name} = {param_name};")

    output.append("            }")
    output.append("")

    # Fields - using PascalCase
    for field in fields:
        type_name, field_name = field.split(" ")
        field_name = to_pascal_case(field_name)
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