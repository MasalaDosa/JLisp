;; Numeric Functions

; Minimum of numeric arguments
;min 1 2 3 => 1
(fun {min & xs} {
  if (== (tail xs) nil) {first xs}
    {do 
      (= {rest} (unpack min (tail xs)))
      (= {item} (first xs))
      (if (< item rest) {item} {rest})
    }
})

; Maximum of numeric arguments
;
(fun {max & xs} {
  if (== (tail xs) nil) {first xs}
    {do 
      (= {rest} (unpack max (tail xs)))
      (= {item} (first xs))
      (if (> item rest) {item} {rest})
    }  
})
