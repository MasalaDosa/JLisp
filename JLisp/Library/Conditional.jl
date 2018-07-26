
;; Conditional Functions
;
(fun {select & cs} {
  if (== cs nil)
    {error "No Selection Found"}
    {if (first (first cs)) {second (first cs)} {unpack select (tail cs)}}
})

(fun {case x & cs} {
  if (== cs nil)
    {error "No Case Found"}
    {if (== x (first (first cs))) {second (first cs)} {
      unpack case (join (list x) (tail cs))}}
})

(def {otherwise} true)

