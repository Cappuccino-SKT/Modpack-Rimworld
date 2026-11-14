# Gera os ícones das ideologias/facções: silhueta branca (o jogo tinge com a cor da
# facção/ideologia) com contorno escuro, para ficar legível no mapa-múndi.
# Saída: Mods/EcosDaQueda/Textures/EcosDaQueda/Icons/*.png (256x256, fundo transparente)
Add-Type -AssemblyName System.Drawing
$out = Join-Path $PSScriptRoot '..\..\Mods\EcosDaQueda\Textures\EcosDaQueda\Icons'
New-Item -ItemType Directory -Force $out | Out-Null
$S = 256
$OUTLINE = 14                                   # espessura extra do contorno (px)
$dark = [System.Drawing.Color]::FromArgb(255, 25, 25, 30)

function Pen([System.Drawing.Color]$c, [float]$w) {
    $p = New-Object System.Drawing.Pen $c, $w
    $p.StartCap = 'Round'; $p.EndCap = 'Round'; $p.LineJoin = 'Round'; return $p
}

# Cada ícone é um scriptblock que recebe ($g, $color, $extra): desenha a forma com a cor
# dada e traços $extra px mais grossos. Chamado 2x: contorno escuro e depois branco.

$icons = [ordered]@{}

# Silêncio: olho fechado com cílios, dentro de um anel
$icons['Silencio'] = {
    param($g, $c, $x)
    $g.DrawEllipse((Pen $c (18 + $x)), 22, 22, 212, 212)
    $g.DrawArc((Pen $c (22 + $x)), 50, 44, 156, 112, 20, 140)
    foreach ($a in 40, 65, 90, 115, 140) {
        $r = [Math]::PI * $a / 180
        $x1 = 128 + 78 * [Math]::Cos($r); $y1 = 100 + 56 * [Math]::Sin($r)
        $x2 = 128 + 106 * [Math]::Cos($r); $y2 = 100 + 90 * [Math]::Sin($r)
        $g.DrawLine((Pen $c (16 + $x)), [float]$x1, [float]$y1, [float]$x2, [float]$y2)
    }
}

# Hélice: dupla hélice com degraus
$icons['Helice'] = {
    param($g, $c, $x)
    $ptsA = @(); $ptsB = @()
    for ($y = 22; $y -le 234; $y += 4) {
        $t = ($y - 22) / 212 * 2 * [Math]::PI * 1.5
        $ptsA += New-Object System.Drawing.PointF ([float](128 + 72 * [Math]::Sin($t))), $y
        $ptsB += New-Object System.Drawing.PointF ([float](128 - 72 * [Math]::Sin($t))), $y
    }
    for ($y = 40; $y -le 220; $y += 24) {
        $t = ($y - 22) / 212 * 2 * [Math]::PI * 1.5
        $dx = 72 * [Math]::Sin($t)
        if ([Math]::Abs($dx) -gt 16) { $g.DrawLine((Pen $c (8 + $x * 0.6)), [float](128 - $dx), $y, [float](128 + $dx), $y) }
    }
    $g.DrawLines((Pen $c (17 + $x * 0.7)), [System.Drawing.PointF[]]$ptsA)
    $g.DrawLines((Pen $c (17 + $x * 0.7)), [System.Drawing.PointF[]]$ptsB)
}

# Forja: engrenagem vazada com ponteiro no centro
$icons['Forja'] = {
    param($g, $c, $x)
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $teeth = 10; $rOut = 116; $rIn = 90
    $pts = @()
    for ($i = 0; $i -lt $teeth; $i++) {
        $a0 = 2 * [Math]::PI * $i / $teeth
        foreach ($o in @(-0.20, -0.12, 0.12, 0.20)) {
            $r = if ([Math]::Abs($o) -lt 0.15) { $rOut } else { $rIn }
            $pts += New-Object System.Drawing.PointF ([float](128 + $r * [Math]::Cos($a0 + $o))), ([float](128 + $r * [Math]::Sin($a0 + $o)))
        }
    }
    $path.AddPolygon([System.Drawing.PointF[]]$pts)
    $path.AddEllipse(74, 74, 108, 108)
    $brush = New-Object System.Drawing.SolidBrush $c
    if ($x -gt 0) { $g.DrawPath((Pen $c $x), $path) }
    $g.FillPath($brush, $path)
    $hub = 22 + $x / 2
    $g.FillEllipse($brush, 128 - $hub, 128 - $hub, 2 * $hub, 2 * $hub)
    $g.DrawLine((Pen $c (18 + $x)), 128, 128, 128, 86)
}

# Coro: diapasão com ondas sonoras
$icons['Coro'] = {
    param($g, $c, $x)
    $p = Pen $c (24 + $x)
    $g.DrawLine($p, 98, 30, 98, 128)
    $g.DrawLine($p, 158, 30, 158, 128)
    $g.DrawArc($p, 98, 98, 60, 60, 0, 180)
    $g.DrawLine($p, 128, 158, 128, 226)
    $r = 18 + $x / 2
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $c), 128 - $r, 228 - $r, 2 * $r, 2 * $r)
    $w = Pen $c (14 + $x)
    $g.DrawArc($w, 44, 34, 72, 100, 120, 120)
    $g.DrawArc($w, 16, 12, 104, 144, 125, 110)
    $g.DrawArc($w, 140, 34, 72, 100, -60, 120)
    $g.DrawArc($w, 136, 12, 104, 144, -55, 110)
}

foreach ($name in $icons.Keys) {
    $b = New-Object System.Drawing.Bitmap $S, $S
    $g = [System.Drawing.Graphics]::FromImage($b)
    $g.SmoothingMode = 'AntiAlias'
    $g.Clear([System.Drawing.Color]::Transparent)
    & $icons[$name] $g $dark $OUTLINE                       # contorno
    & $icons[$name] $g ([System.Drawing.Color]::White) 0    # forma
    $b.Save((Join-Path $out "$name.png"), [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $b.Dispose()
}
Get-ChildItem $out | ForEach-Object { "$($_.Name) $($_.Length) bytes" }
