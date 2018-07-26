; Factorial

(fun {factorial n} {
  do
  (print n)
  (select
    { (== n 0) 0 }
    { (== n 1) 1 }
    { otherwise (* n (factorial (- n 1))) })
})

(print "factorial 10 is" (factorial 10))

