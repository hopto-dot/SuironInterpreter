using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static SuironInterpreter.Expr;
using static SuironInterpreter.Stmt;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SuironInterpreter
{
    class Parser
    {
        private readonly List<Token> tokens;
        private int current = 0; // next token (not character) to be parsed

        private class ParseError : Exception { }

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }
        public List<Stmt> parse()
        {

            List<Stmt> statements = new List<Stmt>();
            while (!isAtEnd())
            {
                statements.Add(declaration());
            }

            return statements;
        }

        private Stmt declaration()
        {
            try
            {
                
                if (match(TokenType.VAR)) return varDeclaration();

                return statement();
            }
            catch (ParseError error)
            {              
                synchronize();
                return null;
            }
        }

        private Stmt varDeclaration()
        {
            string errorMessage = "Expected a variable name.";
            if (peek().Type == TokenType.NUMBER)
            {
                errorMessage += " Variable name cannot be a number.";
            }


            Token name = consume(TokenType.IDENTIFIER, errorMessage);

            Expr initializer = null;
            if (peek().Type == TokenType.STRING)
            {
                error(peek(), "Expected '=' after variable name.");
                return null;
            }
            
            if (match(TokenType.EQUAL))
            {
                initializer = expression();
            }

            string errorString = initializer == null ? $"Expected initialiser after declaration of variable '{name.Lexeme}' but got '{peek().Lexeme}'." : "Expected a ';' after variable declaration.";

            consume(TokenType.SEMICOLON, errorString);
            return new Stmt.Var(name, initializer);
        }

        #region Program BNF
        //program   := statement* EOF;

        //statement := exprStmt
        //          | printStmt ;

        //exprStmt  := expression ";" ;
        //printStmt := "print" expression ";" ;
        #endregion

        private Stmt statement()
        {
            if (match(TokenType.FOR))
            {
                return forStatement();
            }
            if (match(TokenType.IF))
            {
                return ifStatement();
            }
            if (match(TokenType.PRINT))
            {
                return printStatement();
            }
            if (match(TokenType.WHILE))
            {
                return whileStatement();
            }
            if (match(TokenType.LEFT_BRACE))
            {
                return new Stmt.Block(block());
            }
            return expressionStatement();
        }

        private Stmt forStatement()
        {
            consume(TokenType.LEFT_PAREN, "'(' after 'for' is expected");

            Stmt initialiser;
            if (match(TokenType.SEMICOLON))
            {
                initialiser = null;
            }
            else if (match(TokenType.VAR))
            {
                initialiser = varDeclaration();
            }
            else
            {
                initialiser = expressionStatement();
            }

            Expr condition = null;
            if (!check(TokenType.SEMICOLON))
            {
                condition = expression();
            }
            consume(TokenType.SEMICOLON, "';' after loop condition is expected");

            Expr increment = null;
            if (!check(TokenType.RIGHT_PAREN))
            {
                increment = expression();
            }
            consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

            Stmt body = statement();

            if (increment != null)
            {
                body = new Stmt.Block( new List<Stmt> { body, new Stmt.Expression(increment) } );
            }

            if (condition == null) condition = new Expr.Literal(true);
            
            body = new Stmt.While(condition, body);

            if (initialiser != null)
            {
                body = new Stmt.Block(new List<Stmt> { initialiser, body });
            }

            return body;
        }

        private Stmt whileStatement()
        {
            consume(TokenType.LEFT_PAREN, "'(' after 'while' is expected");
            Expr condition = expression();
            consume(TokenType.RIGHT_PAREN, "')' after condition is expected");
            Stmt body = statement();

            return new Stmt.While(condition, body);
        }

        private Stmt ifStatement()
        {
            consume(TokenType.LEFT_PAREN, "'(' after the 'if' is expected.");
            Expr condition = expression();
            consume(TokenType.RIGHT_PAREN, "')' after if condition is expected.");

            Stmt thenBranch = statement();
            Stmt elseBranch = null;
            if (match(TokenType.ELSE))
            {
                elseBranch = statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private List<Stmt> block()
        {
            List<Stmt> statements = new List<Stmt>();

            while (!check(TokenType.RIGHT_BRACE) && !isAtEnd())
            {
                statements.Add(declaration());
            }

            consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Stmt printStatement()
        {
            Expr value = expression();
            consume(TokenType.SEMICOLON, "Expected ';' at the end of the line.");
            return new Stmt.Print(value);
        }
        private Stmt expressionStatement()
        {
            Expr expr = expression();
            consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }


        #region Expression BNF
        // extended BNF grammar representation:

        // expression := equality ;
        // equality   := comparison ( ( "!=" | "==" ) comparison )* ;
        // comparison := term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
        // term       := factor ( ( "-" | "+" ) factor )* ;
        // factor     := unary ( ( "/" | "*" ) unary )* ;
        // unary      := ( "!" | "-" ) unary
        //               | primary ;
        // primary    := NUMBER | STRING | "true" | "false" | "nil"
        //               | "(" expression ")" ;
        #endregion
        private Expr expression()
        {
            // return equality();
            return assignment();
        }

        private Expr or()
        {
            Expr expr = and();

            while (match(TokenType.OR))
            {
                Token @operator = previous();
                Expr right = and();
                expr = new Expr.Logical(expr, @operator, right);
            }

            return expr;
        }

        private Expr and()
        {
            Expr expr = equality();

            while (match(TokenType.AND))
            {
                Token @operator = previous();
                Expr right = equality();
                expr = new Expr.Logical(expr, @operator, right);
            }

            return expr;
        }

        private Expr assignment()
        {
            Expr expr = or();
            
            // Expr expr = equality(); // this turns the left side into an expression

            if (match(TokenType.EQUAL)) // if after that expression is `=`
            {
                Token equals = previous(); // we set the token which comes before the `=` to `equals` (the identifier)
                Expr value = assignment(); // we now run assignment method again which returns the right side of the expression (the value)

                if (expr is Expr.Variable) // if the left side is a variable (we're expecting it to be specfically an identifier)
                { 
                    Token name = ((Expr.Variable)expr).Name;
                    return new Expr.Assign(name, value);
                }

                error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr equality()
        {
            Expr expr = comparison();

            while (match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token @operator = previous();
                Expr right = comparison();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }
        private Expr comparison()
        {
            Expr expr = term();

            while (match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token @operator = previous();
                Expr right = term();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }
        private Expr term()
        {
            Expr expr = factor();

            while (match(TokenType.MINUS, TokenType.PLUS, TokenType.AMPERSAND))
            {
                Token @operator = previous();
                Expr right = factor();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }
        private Expr factor()
        {
            Expr expr = unary();

            while (match(TokenType.SLASH, TokenType.STAR))
            {
                Token @operator = previous();
                Expr right = unary();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }
        private Expr unary()
        {
            if (match(TokenType.BANG, TokenType.MINUS))
            {
                Token @operator = previous();
                Expr right = unary();
                return new Expr.Unary(@operator, right);
            }

            return primary();
        }
        private Expr primary()
        {
            if (match(TokenType.FALSE)) return new Expr.Literal(false);
            if (match(TokenType.TRUE)) return new Expr.Literal(true);
            if (match(TokenType.NIL)) return new Expr.Literal(null);

            if (match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(previous().Literal);
            }

            if (match(TokenType.IDENTIFIER))
            {
                return new Expr.Variable(previous());
            }

            if (match(TokenType.LEFT_PAREN))
            {
                Expr expr = expression();
                consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            // throw error(peek(), "Expect expression.");

            //throw error(
            //    peek(),
            //    $"Expected expression, but found '{peek().Lexeme}'. " +
            //    "An expression should start with 'true', 'false', 'nil', a number, a string, or '('."
            //    );

            // Look at the previous token to provide context
            Token prev = current > 0 ? tokens[current - 1] : null;
            Token curr = peek();

            string message;
            if (prev != null)
            {
                switch (prev.Type)
                {
                    case TokenType.PLUS:
                    case TokenType.MINUS:
                    case TokenType.STAR:
                    case TokenType.SLASH:
                        message = $"Expected expression after operator '{prev.Lexeme}'";
                        break;
                    case TokenType.LEFT_PAREN:
                        message = "Expected expression after '('";
                        break;
                    default:
                        message = $"Expected expression before '{curr.Lexeme}'";
                        break;
                }
            }
            else
            {
                message = $"Expected expression at start of line but found '{curr.Lexeme}'";
            }

            throw error(peek(), message);
        }

        private Token consume(TokenType type, string message)
        {
            if (check(type)) return advance();

                throw error(peek(), message);
            return null;
        }

        private ParseError error(Token token, string message)
        {
            Program.error(token.Line, message);
            return new ParseError();
        }

        private void synchronize()
        {
            advance();

            while (!isAtEnd())
            {
                if (previous().Type == TokenType.SEMICOLON) return;

                switch (peek().Type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                advance();
            }
        }
        private bool match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (check(type))
                {
                    advance();
                    return true;
                }
            }

            return false;
        }
        private bool check(TokenType type)
        {
            if (isAtEnd())
            {
                return false;
            }
            else
            {
                return peek().Type == type;
            }
        }
        private Token advance()
        {
            if (!isAtEnd())
            {
                current++;
            }
            return previous();
        }
        private bool isAtEnd()
        {
            return peek().Type == TokenType.EOF;
        }
        private Token peek()
        {
            return tokens[current];
        }
        private Token previous()
        {
            return tokens[current - 1];
        }
    }

}
