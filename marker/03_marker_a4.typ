#import "@preview/cetz:0.5.2": canvas, draw
#set page(paper: "a4", margin: 15mm)
#set text(font: "FreeSans", size: 10pt, lang: "es")
#set par(leading: 3pt)
#align(center)[
  #text(size: 15pt, weight: "bold")[CHUPACABRAS · Prueba AprilTag 03]

  tagStandard41h12 · ID 0 · A4 · imprimir al 100 %
]
#v(15mm)
#let pattern = (
  "..#....##", "#########", ".#.....##", "##.....#.", "##.##..##",
  "##.#.#.#.", ".#.....##", "#########", "..#.##.##",
)
#align(center, canvas(length: 1mm, {
  import draw: *
  rect((0,0), (180,180), fill: white, stroke: none)
  for (y, row) in pattern.enumerate() {
    for (x, pixel) in row.clusters().enumerate() {
      if pixel == "#" { rect((x*20, (8-y)*20), ((x+1)*20, (9-y)*20), fill: black, stroke: none) }
    }
  }
}))
#v(14mm)
#align(center, canvas(length: 1mm, {
  import draw: *
  line((0,0), (100,0), stroke: .5pt)
  line((0,-2), (0,2), stroke: .5pt)
  line((100,-2), (100,2), stroke: .5pt)
}))
#align(center)[Regla: *100 mm* entre las dos marcas.]
#text(size: 9pt)[
  El cuadrado de detección mide *100 × 100 mm*: borde interior del marco negro
  continuo, alrededor del centro. El dibujo completo mide *180 × 180 mm*.
  Conserva la hoja plana y sus márgenes blancos de 15 mm como mínimo.
  No uses «ajustar a página». No es un QR ni el marcador definitivo del stand.
]
