JLisp Readme

This is an implementation of a Lisp like language based very much on that described
on the excellent http://www.buildyourownlisp.com/ by Daniel Holden.
If you are interested in c, or lisp I can't 'recommend it enough.

This version is written in c# and uses my own basic parser library, QDP

Its not particular performant but rather was written as a learning exercise.
Any errors and omissions are my own.

To run the examples
JLISP> load "Examples/factorial.jl"
JLISP> load "Examples/fibonacci.jl"
JLISP> load "Examples/helloworld.jl"

Built In Functions
------------------

1. List Functions

list
----
Returns a q-expression from its arguments:
> list 1 2 3 => {1 2 3}

head
----
Returns the head of a q-expression list:
> head {1 2 3} => {1}

tail
----
Returns the tail of a q-expression list:
>tail {1 2 3} => {2 3}

eval
----
Evaluates a q-expression by converting to an s-expression:
> eval {+ 1 2 3} => (+ 1 2 3) => 6

join
----
Joins multiple q-expression lists together:
> join {1 2 3} {4 5 6} {7 8 9} => {1 2 3 4 5 6 7 8 9}

2. Function Declaration

\
-
Defines an anonymous function from formals and a body:
> \ {x y} {+ x y}
Which can be invoked as follows:
> (\ {x y} {+ x y}) 1 2 => 3
Or assigned to thr global environment as follows:
> def {add-func} (\ {x y} {+ x y})
> add-func 1 2 => 3

See also the 'fun' library function.

3. Mathematical Functions

+, -, *, /, %, ^
----------------
Plus, Minus, Multiply, Divide, Modulus and Power
These all operate on multiple numeric (64bit integer) values:
> + 1 2 3 => 6
Note that - followed by a single number negates that number:
> - 5 => -5

4. Ordinals and Comparisons

>, <, >=, <=, ==, !=
--------------------
Greater than, Less than, Greater than or equal, Less than or equal, Equal, Not Equal:
> > 2 1 => 1
> < 2 1 => 0

if
--
Evaluates either one or the other q-expression:
> if (> 2 1) {"True"}  {"False"} => "True"


5. Environment

def
---
Assigns a value to a key in the root environment:
> def {myName} "Jay"
> myName => "Jay"

=
-
Assigns a value to a key in the current environment:
> = {x} 1

list-env
--------
Returns a q-expression containing key value pairs for each each and value in the environment:
> list-env 0 returns for current environment only
> list-env 1 returns for parent environments as well.
 
load
----
Loads a JLisp file:
> load "Examples/Helloworld.jl"

print
-----
Prints out it's arguments:
> print (+ 1 2 3) => 6
> print "Hel\"lo" => "Hel\"lo"

show
----
Prints out arguments leaving any strings unescaped:
show "Hel\"lo" => Hel"lo

error
-----
Generates a JLisp error value:
error "Oh no" => Error: Oh no

Standard Library
----------------

The standard library of functions is based on that offered by Daniel on his website.

1. Atoms

Declares nil, true and false symbols as aliases for {}, 1 and 0.

2. Functions

fun
---
A function to create functions using an easier syntax that is used throughout the library:
fun {name args} (body)
> fun {add-three a b c} {+ a b c}
> add-three 1 2 3 => 6

unpack/curry
------------
Allows a call a function taking mulitiple arguments with a single list:
> curry + {5 6 7} => 18
> def {curry-add} (curry +)
> curry-add {5 6 7} => 18

pack/uncurry
------------
The opposite of currying - allows us to call a function accepting list with multiple args
> uncurry head 5 6 7 => {5}

let
---
Open a new scope - defines a lamba function with a caller defined body (\ {_} b) which is then invoked with nil.
Caller can work inside this body, including making assignments to the local scope *without* cluttering the global scope.
This is particularly useful when combined with 'do':
> let {= {x} 77} => ()
> x => Undefined symbol

do
--
Perform Several things in Sequence then return the last one
do (print "hello") (print "goodbye") (+ 1 2 3)
This is particularly useful when combined with 'let':.
> let {do (= {x} 123) (= {y} 456) (+ x y)} => 579
x => Undefined symbol

3. Logical

Implementations of not, or and and using our convention of 0 == false and 1 == true:
> not 1 => 0
> not 0 => 1
> or 0 0 => 0
> or 1 0 => 1 etc.

4. List

cons
----
Prepend the value 'item' to the q-expression list:
> cons 1 {2 3} => {1 2 3}

init
----
Return all of list but last element:
> init {1 2 3} => {1 2}

len
---
Recusively determine the length of a list:
> len {1 2 3} => 3

rev
---
Reverses a list
> rev {1 2 3} => {3 2 1}

first, second, last, nth
------------------------
Returns the first, second, last and nth (0 based) from a list:
> first {1 2 3 4} => 1
> second {1 2 3 4} => 2
> last {1 2 3 4} => 4
> nth 2 {1 2 3 4} =? 3

member
------
Determines if an element is in a list:
> member 3 {1 2 3} => 1
> member 4 {1 2 3} => 0

indexof, lastindexof
--------------------
; Determine the index of an element in a list or -1 if not present:
> indexof 2 {1 2 3 2} => 1

map
---
Apply a function to list:
> map print {"hello" "goodbye"} => "hello" "goodbye" {() ()}

filter
------
Apply a predicate to a list:
fun {is-even n} {== (% n 2) 0}
filter is-even {1 2 3 4 5 6 7 8} => {2 4 6 8}

foldl, foldr
------------
Fold left and right to reduce or aggregate a list:
foldl + 0 {1 2 3 4} => 10 
foldr - 0 {1 2 3 4} => 10

The difference is in the order, for example given the following function:
fun {plus a b} {do (print a b) (+ a b)}
fold left will produce the following:
foldl plus 0 {1 2 3 4}
0 1
1 2
3 3
6 4
10

Whereas foldr will start at the opposite end:
4 0
3 4
2 7
1 9
10

Further examples
; sum {1 2 3}
> (fun {sum qexpr} {foldl + 0 qexpr})
> sum {1 2 3}
> (fun {product qexpr} {foldl * 1 qexpr})
> product { 1 2 3}

take
----
Take N items from a list:
> take 5 {1 2 3 4 5 6 7 8 9 10} => {1 2 3 4 5}

drop
----
Drop N items from a list:
> drop 5 {1 2 3 4 5 6 7 8 9 10} => {6 7 8 9 10}

split
-----
Split a list at N
> split 5 {1 2 3 4 5 6 7 8 9 10} => {1 2 3 4 5} {6 7 8 9 10}}

zip
---
Zip two lists together into a list of pairs:
> zip {"a" "b"} {1 2} => {{"a" 1} {"b" 2}}

unzip
-----
Unzip a list of pairs into two lists
> unzip {{"a" 1} {"b" 2}} => {{"a" "b"} {1 2}}

lookup
------
Find element in list of pairs
> lookup "b" (zip {"a" "b" "c"} {1 2 3}) => 2

5. Numeric

min, max
--------
Returns the minimum or maximum from args:
> min 12 3 56 2 3 => 2

6. Conditional

select and case
---------------
Powerful switching constructs:

> = {i} 6
> select {(== i 0) "none"} {(and (>= i 1) (<= i 3)) "some"} {otherwise "many"} => "many"
> case i {0 "Zero"} {1 "One"} {2 "Two"} {3 "Three"} {4 "Four"} => Error: No Case Found
> = {i} 2
> case i {0 "Zero"} {1 "One"} {2 "Two"} {3 "Three"} {4 "Four"} => "Two"
  