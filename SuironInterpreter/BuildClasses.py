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
    keywords = ["operator"]
    if name.lower() in keywords:
        return "@" + name
    return name

def define_visitor(writer: str, base_name: str, types: List[str]) -> str:
    output = []
    # Define visitor interface
    output.append(f"        public interface IVisitor<R>")
    output.append("        {")

    # Add visit method for each type
    for type_def in types:
        type_name = type_def.split(":")[0].strip()
        type_name = to_pascal_case(type_name)
        output.append(f"            R Visit{type_name}{base_name}({type_name} {to_camel_case(base_name)});")

    output.append("        }")
    output.append("")
    return "\n".join(output)

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
        param_name = escape_keyword(to_camel_case(field_name))
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

    # Fields
    for field in fields:
        type_name, field_name = field.split(" ")
        field_name = to_pascal_case(field_name)
        output.append(f"            public readonly {type_name} {field_name};")

    # Visitor pattern implementation
    output.append("")
    output.append("            public override R Accept<R>(IVisitor<R> visitor)")
    output.append("            {")
    output.append(f"                return visitor.Visit{class_name}{base_name}(this);")
    output.append("            }")

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

    # Add the visitor interface
    visitor_interface = define_visitor("", base_name, types)
    output.append(visitor_interface)

    # Add the base accept method
    output.append("        public abstract R Accept<R>(IVisitor<R> visitor);")
    output.append("")

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
        "Unary    : Token Operator, Expr Right",
        "Variable : Token name"
    ])

    define_ast(output_dir, "Stmt", [
        "Expression   : Expr expression",
        "Print : Expr expression",
        "Var : Token name, Expr initialiser",
        "Assign : Toke name, Expr value"
    ])

if __name__ == "__main__":
    main()

# The python code generation script, generates lots of classes that inherit from Expr (these classes are all types/subclasses of the Expression class).

# It also defines a "Visitor" interface which requires a "Visit_classname" method for each of the Expression subclasses, each taking a parameter of the subclass to be evaluation.
# The purpose of each of the Visit functions is it evaluate each type of expression.

# All Expression subclasses must implement an "Accept" function which points to the correct visit function in the visitor class.
 
# In use (code interpretation), instances of expressions will exist, and when they need to be evaluated, you run the Accept function on them,
# which runs (points to the correct) evaluate function in the visitor class.