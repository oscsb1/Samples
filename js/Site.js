function settitulopagina(titulo) {
    try {


        if (titulo != undefined) {
            var obj = document.getElementById('titulopagina')
            if (obj != null && obj != undefined) {
                obj.innerHTML = titulo;
                obj.style.paddingTop = "20px";
            }
        }
    }
    catch { }
}



function ajustalayoutperiodo() {
    var x = document.getElementsByClassName("mylayoutperiodo");
    if (x != null && x.length == 1) {
        x[0].style.maxHeight = (window.innerHeight - x[0].offsetTop - 30) + "px";
    }
}



function setgapnav() {
    var nav = document.getElementById("navbar");
    var gap = document.getElementById("gapnav");
    if (nav != null && gap != null) {
        var s = nav.clientHeight + 'px';
        if (s != gap.style.height)
            gap.style.height = s;
    }
}


function AjustaFont(el, ct) {

    if (ct > 5)
        return;
    ct++;
    if (el != null) {
        el.style = "font-size:10px;max-height:20px;margin-top:0px;padding-top:0.3px;background-color:whitesmoke";
        var d = el.children;
        if (d != null && d.length > 0) {
            for (j = 0; j < d.length; j++) {
                AjustaFont(d[j], ct);
            }
        }
    }

}

function AjustAutoSelect(el) {

    var x = document.getElementsByClassName("myautocomplete");
    if (x != null && x.length > 0) {
        for (i = 0; i < x.length; i++) {
            AjustaFont(x, 1);
        }
    }

    /*
        var y = document.getElementById(el);
        if (y != null) {
            AjustaFont(y, 1);
        }
        */
}


function ajustatable(id, gap) {
    if (gap == undefined) {
        gap = 30;
    }

    var mt = document.getElementById("modaltable")
    if (mt != null) {
        return;
    }

    var x = document.getElementsByClassName("mytableresp");
    if (x != null && x.length > 0) {
        for (i = 0; i < x.length; i++) {
            if (x[i].id == "tabler130") { gap = 130; }
            if (x[i].id == "tabler170") { gap = 170; }
            x[i].style.maxHeight = (window.innerHeight - x[i].offsetTop - gap) + "px";
            x[i].style.minHeight = (window.innerHeight - x[i].offsetTop - gap) + "px";
            x[i].style.height = (window.innerHeight - x[i].offsetTop - gap) + "px";
        }
    }
    var y = document.getElementsByClassName("mytablerespgap");
    if (y != null && y.length > 0) {
        for (i = 0; i < y.length; i++) {
            y[i].style.maxHeight = (window.innerHeight - y[i].offsetTop - 70) + "px";
            AjustaStyleFiltro(y[i]);
        }
    }
    var z = document.getElementsByClassName("mytablerespgapfix");
    if (z != null && z.length > 0) {
        for (i = 0; i < z.length; i++) {
            AjustaStyleFiltro(z[i]);
        }
    }

    ajustaposicaoMenu();

    ajustalayoutperiodo();

    ajustbotoes();

}

function ajustbotoes() {


    var z = document.getElementsByClassName("mybtshrink");
    if (z != null && z.length > 0) {
        var sb = document.getElementById("sidebar");
        for (i = 0; i < z.length; i++) {
            if ((window.innerWidth < 600 && sb != null && sb.offsetLeft > -1) || (window.innerWidth < 400)) {
                if (z[i].clientWidth > 30) {
                    if (z[i].title == "") {
                        z[i].title = z[i].lastChild.textContent;
                    }
                    z[i].lastChild.textContent = "";
                    z[i].style = "width:25px;padding-left:6px;";
                }
            }
            else {
                if (window.innerWidth > 600 || sb != null && sb.offsetLeft < -1)
                    if (z[i].clientWidth < 30) {
                        if (z[i].title == "") {
                            z[i].title = z[i].lastChild.textContent;
                        }
                        z[i].lastChild.textContent = z[i].title;
                        z[i].style = "width:" + z[i].id.substring(5, 8) + "px" + ";padding-left:3px; ";
                    }
            }
        }
    }
}


function ajustaposicaoMenu() {
    var nv = document.getElementById("navbartop");
    var sb = document.getElementById("navmenu");
    if (sb != null && nv != null) {
        sb.style.marginTop = nv.clientHeight - 0 + "px";
    }

    var bd = document.getElementById("gapnav");
    if (bd != null && nv != null) {
        bd.style.marginTop = nv.clientHeight + "px";
    }

    ajustbotoes();
}

function scrolltbcolfix(id) {

    var el = document.getElementById(id);
    if (el == null) {
        return;
    }
    return;
    setTimeout(ajustafixedcol, 1000);

}

function ajustafixedcol() {
    var x = document.getElementsByClassName("fixedcolhdr");
    if (x != null && x.length > 0) {
        for (i = 0; i < x.length; i++) {
            x[i].style.top = Math.trunc(el.scrollTop) - 1 + "px";
            x[i].style.let = Math.trunc(el.scrollLeft) - 1 + "px";
            x[i].style = "height:33px;max-height=33px;background-color:red;";
        }
    }
    var x = el.getElementsByClassName("fixedcolhdr2");
    if (x != null && x.length > 0) {
        for (i = 0; i < x.length; i++) {
            x[i].style.top = Math.trunc(el.scrollTop) - 1 + "px";
        }
    }
}

function scrolltb(id) {

    var y = document.getElementById(id);
    if (y != null) {
        AjustaFiltroElemento(y);
    }

}


function AjustaFiltro(el) {

    if (el == null || el == undefined) {

        var x = document.getElementsByClassName("mytableresp");
        if (x != null && x.length > 0) {
            for (k = 0; k < x.length; k++) {
                AjustaFiltroElemento(x[k]);
            }
        }

        var y = document.getElementsByClassName("mytablerespgap");
        if (y != null && y.length > 0) {
            for (i1 = 0; i1 < y.length; i1++) {
                AjustaFiltroElemento(y[i1]);
            }
        }

        var z = document.getElementsByClassName("mytablerespgapfix");
        if (z != null && z.length > 0) {
            for (i2 = 0; i2 < z.length; i2++) {
                AjustaFiltroElemento(z[i1]);
            }
        }

    }
    else {
        AjustaFiltroElemento(el)
    }
}

function AjustaFiltroElemento(el) {

    var rateio = false;

    var h = el.getElementsByTagName("thead");
    if (h != null && h.length > 0) {
        for (k = 0; k < h.length; k++) {
            h[k].style.top = Math.trunc(el.scrollTop) - 0 + "px";
        }
    }
    var h = el.getElementsByTagName("th");
    if (h != null && h.length > 0) {
        for (k = 0; k < h.length; k++) {
            if (h[k].classList.contains("fixedcolhdr3")) {
                h[k].style.top = Math.trunc(el.scrollTop) + 32 + "px";
            }
            if (h[k].classList.contains("sticky-header-filter")) {
                h[k].style.top = Math.trunc(el.scrollTop) + 60 + "px";
            }
            else {
                h[k].style.top = Math.trunc(el.scrollTop) - 0 + "px";
            }
        }
    }
    var x = el.getElementsByClassName("sticky-header");
    if (x != null && x.length > 0) {
        for (i = 0; i < x.length; i++) {
            //        x[i].style = "height:33px;max-height=33px;background-color:whitesmoke;";
            x[i].style.top = Math.trunc(el.scrollTop) - 3 + "px";
        }
    }

    var x = el.getElementsByClassName("sticky-header-center");
    if (x != null && x.length > 0) {
        for (i = 0; i < x.length; i++) {
            x[i].style = "height:33px;max-height=33px;background-color:whitesmoke;";
            x[i].style.top = Math.trunc(el.scrollTop) - 3 + "px";
        }
    }
    var x = el.getElementsByClassName("sticky-header-left");
    if (x != null && x.length > 0) {
        for (i = 0; i < x.length; i++) {
            x[i].style = "height:33px;max-height=33px;background-color:whitesmoke";
            x[i].style.top = Math.trunc(el.scrollTop) - 3 + "px";
        }
    }
    var x = el.getElementsByClassName("sticky-header-right");
    if (x != null && x.length > 0) {
        for (i = 0; i < x.length; i++) {
            x[i].style = "height:33px;max-height=33px;";
            x[i].style.top = Math.trunc(el.scrollTop) - 3 + "px";
        }
    }

    var f = document.getElementsByClassName("sticky-header-filter");
    if (f != null && f.length > 0) {
        for (i = 0; i < f.length; i++) {
            if (!f[i].classList.contains("display-block")) {
                f[i].classList.add("display-block");
            }
            f[i].style.top = Math.trunc(el.scrollTop) - 3 + "px";
            var d = f[i].children;
            if (d != null && d.length > 0) {
                for (j = 0; j < d.length; j++) {
                    d[j].style = "font-size:12px;max-height:26px;padding-left:2px;background-color:white;";
                }
            }
        }
    }

    var h = el.getElementsByTagName("th");
    if (h != null && h.length > 0) {
        for (k = 0; k < h.length; k++) {
            if (h[k].classList.contains("fixedcolhdr3")) {
                h[k].style.top = Math.trunc(el.scrollTop) + 32 + "px";
            }
            else {
                if (h[k].classList.contains("sticky-header-filter")) {
                    h[k].style.top = Math.trunc(el.scrollTop) - 3 + "px";
                }
                else {
                    h[k].style.top = Math.trunc(el.scrollTop) - 0 + "px";
                }
            }
        }
    }

}

function AjustaStyleFiltro(el) {

    var f = document.getElementsByClassName("sticky-header-filter");
    if (f == null || f.length == 0) { return; }

    for (i = 0; i < f.length; i++) {
        var d = f[i].children;
        if (d != null && d.length > 0) {
            for (j = 0; j < d.length; j++) {
                d[j].style = "font-size:12px;max-height:25px;padding-left:2px;position: sticky;";
            }
        }
    }

}


function AjustaStyleAllFiltro() {
    var f = document.getElementsByClassName("sticky-header-filter");
    if (f == null || f.length == 0) { return; }

    for (i = 0; i < f.length; i++) {
        var d = f[i].children;
        if (d == null || d.length == 0) { return; }
        for (j = 0; j < d.length; j++) {
            d[j].style = "font-size:12px;max-height:25px;padding-left:2px;";
        }
    }
}


function setfocustab(nome) {

    var x = document.getElementsByClassName("mytab");
    if (x != null && x.length > 0) {
        for (i = 0; i < x.length; i++) {
            if (x[i].innerText == nome) {
                var d = x[i].children;
                if (d == null || d.length == 0) { return; }
                for (j = 0; j < d.length; j++) {
                    d[j].style = " background:#4e226c;color:white;border-radius:5px";
                }

            }
            else {
                var d = x[i].children;
                if (d == null || d.length == 0) { return; }
                for (j = 0; j < d.length; j++) {
                    d[j].style = " background:whitesmoke;color:#4e226c;borber-style:none;";
                }

            }
        }
    }

}

function NavigateTo(url) {
    // window.open(window.location.href = url, '_blank');
    window.location.href = url;
}


function mapaloaded() {
    var cv = document.getElementById("cv");
    var ctx = cv.getContext("2d");
    var img = document.getElementById("mapa");
    cv.width = img.naturalWidth;
    cv.height = img.naturalHeight;
    ctx.drawImage(img, 0, 0);

    var ocv = document.getElementById("ocv");
    var octx = ocv.getContext("2d");
    ocv.width = img.naturalWidth;
    ocv.height = img.naturalHeight;
    octx.drawImage(img, 0, 0);

}


function copymapa() {
    var cv = document.getElementById("cv");
    var ctx = cv.getContext("2d");
    var img = ctx.getImageData(1, 1, cv.width, cv.height);
    var ocv = document.getElementById("ocv");
    var octx = ocv.getContext("2d");
    octx.putImageData(img, 1, 1);
}




function drawunidade(fontcolor, ox, oy, x, y, nro, cor, zoom, fontsize, circulosize) {

    if (ox > 0) {

        var gap = 15;
        var recs = 28;
        if (circulosize < 8) {
            gap = 7;
            recs = 15;
        }

        var oc = document.getElementById("ocv");
        var octx = oc.getContext("2d");
        octx.scale(1, 1);
        var imgData = octx.getImageData(ox * zoom - gap * zoom, oy * zoom - gap * zoom, recs * zoom, recs * zoom);
        var c1 = document.getElementById("cv");
        var ctx1 = c1.getContext("2d");
        octx.scale(1, 1);
        ctx1.putImageData(imgData, ox * zoom - gap * zoom, oy * zoom - gap * zoom);
    }

    if (x + y == 0) {
        return;
    }
    if (circulosize < 6) {
        circulosize = 10;
    }
    if (fontsize < 6) {
        fontsize = 11;
    }

    var cv = document.getElementById("cv");
    var ctx = cv.getContext("2d");
    ctx.beginPath();
    ctx.font = fontsize + "px Verdana";
    ctx.scale(1, 1);
    ctx.arc(x, y, circulosize, 0, 2 * Math.PI);
    ctx.fillStyle = cor;
    ctx.textAlign = "center";
    ctx.fill();
    ctx.stroke();
    ctx.scale(1, 1);
    ctx.fillStyle = fontcolor;
    if (circulosize < 10) {
        ctx.fillText(nro, x, y + 3, 20);
    }
    else {
        ctx.fillText(nro, x, y + 4, 20);
    }

}



function focuscell(oid, id) {
    var c = document.getElementById(oid);
    if (oid > 0) {
        c.style = " background:white;color:black;";
    }

    var c2 = document.getElementById(id);
    if (id > 0) {
        c2.style = " background:#4e226c;color:whitesmoke;";
    }
}

function drawdlg(ox, oy, x, y, st1, st2, cor1, area, zoom) {

    var c = document.getElementById("cv");
    if (c == null || c == undefined) {
        return;
    }

    if (ox > 0) {

        if (ox + 150 > c.width) {
            var oc = document.getElementById("ocv");
            var octx = oc.getContext("2d");
            octx.scale(1, 1);
            var imgData = octx.getImageData(ox * zoom - 200 * zoom, oy * zoom - 2, 200 * zoom, 200 * zoom);
            var ctx1 = c.getContext("2d");
            ctx1.putImageData(imgData, ox * zoom - 200 * zoom, oy * zoom - 2);
        }
        else {
            var oc = document.getElementById("ocv");
            var octx = oc.getContext("2d");
            octx.scale(1, 1);
            var imgData = octx.getImageData(ox * zoom + 10 * zoom, oy * zoom - 2, 200 * zoom, 200 * zoom);
            var ctx1 = c.getContext("2d");
            ctx1.putImageData(imgData, ox * zoom + 10 * zoom, oy * zoom - 2);
        }
    }


    if (x + y == 0) {
        return;
    }

    var ctx = c.getContext("2d");
    if (x + 150 < c.width) {

        x = x + 10;

        ctx.beginPath();
        ctx.scale(1, 1);
        ctx.lineWidth = 2;

        ctx.lineTo(x, y);
        ctx.arcTo(x + 130, y, x + 130, y + 60, 15);

        ctx.arcTo(x + 130, y + 45, x + 75, y + 50, 15);
        ctx.lineTo(x + 38, y + 47);
        ctx.arcTo(x + 23, y + 45, x + 25, y + 0, 15);

        ctx.lineTo(x + 23, y + 15);

        ctx.lineTo(x, y);

        ctx.fillStyle = "ghostwhite";
        ctx.fill();
        ctx.textAlign = "left";
        ctx.font = "11px Verdana";
        ctx.fillStyle = cor1;
        ctx.fillText(st1, x + 35, y + 14, 100);
        ctx.font = "11px Verdana";
        ctx.fillStyle = "black";
        ctx.fillText(st2, x + 35, y + 27, 100);
        ctx.fillStyle = "black";
        ctx.fillText(area, x + 35, y + 40, 100);
        ctx.stroke();

    }
    else {
        x = x - 10;
        ctx.beginPath();
        ctx.lineWidth = 2;
        ctx.moveTo(x - 130, y);
        ctx.lineTo(x, y);
        ctx.arcTo(x - 150, y, x - 150, y + 70, 15);
        ctx.arcTo(x - 150, y + 45, x - 80, y + 48, 15);
        ctx.arcTo(x - 35, y + 47, x - 35, y + 35, 15)
        ctx.lineTo(x - 35, y + 20);
        ctx.lineTo(x, y);
        ctx.fillStyle = "lightyellow";
        ctx.fill();
        ctx.textAlign = "left";
        ctx.font = "11px Verdana";
        ctx.fillStyle = cor1;
        ctx.fillText(st1, x - 130, y + 14, 100);
        ctx.fillStyle = "black";
        ctx.fillText(st2, x - 130, y + 27, 100);
        ctx.fillText(area, x + 35, y + 40, 100);

        ctx.stroke();
    }
}

function mousemove(event) {

    var c = document.getElementById("posicao");
    if (c == null || c == undefined) {
        return;
    }
    c.style = "font-size:12px";
    c.innerHTML = event.offsetX + " x " + event.offsetY;

}

function zoomimagem(x) {
    var cv = document.getElementById("cv");
    var ctx = cv.getContext("2d");
    var img = document.getElementById("mapa");
    cv.width = img.naturalWidth * x;
    cv.height = img.naturalHeight * x;
    ctx.scale(x, x);
    ctx.drawImage(img, 0, 0);

    var ocv = document.getElementById("ocv");
    var octx = ocv.getContext("2d");
    ocv.width = img.naturalWidth * x;
    ocv.height = img.naturalHeight * x;
    octx.scale(x, x);
    octx.drawImage(img, 0, 0);
}


function clearcanvas(id) {
    var cv = document.getElementById(id);
    if (cv != undefined) {
        var ctx = cv.getContext("2d");
        ctx.clearRect(0, 0, cv.width, cv.height);
    }
}

function drawintervalo(id, cor, y, h, semagenda, clear) {
    if (y == 0) {
        y = 1;
    }
    var cv = document.getElementById(id);
    var ctx = cv.getContext("2d");
    if (clear) {
        ctx.clearRect(0, 0, cv.width, cv.height);
    }
    if (semagenda) {
        ctx.fillStyle = "red";
    }
    else {
        ctx.fillStyle = cor;
    }
    ctx.fillRect(0, y, 148, h);
}

function drawhora(hora, y, h, posicao, expediente, clear) {
    if (y == 0) {
        y = 1;
    }
    var cv = document.getElementById("hora");
    var ctx = cv.getContext("2d");
    if (clear) {
        ctx.clearRect(0, 0, cv.width, cv.height);
    }
    if (expediente) {
        ctx.fillStyle = "whitesmoke";
    }
    else {
        ctx.fillStyle = "white";
    }
    ctx.fillRect(1, y, cv.width - 5, h);
    ctx.beginPath();
    ctx.lineWidth = 0.5;
    ctx.moveTo(1, y);
    ctx.lineTo(cv.width, y);
    ctx.stroke();

    if (h > 30) {
        ctx.beginPath();
        ctx.lineWidth = 0.5;
        ctx.moveTo(25, y + h - 30);
        ctx.lineTo(50, y + h - 30);
        ctx.stroke();
    }

    if (posicao == 2) {
        ctx.beginPath();
        ctx.moveTo(1, y + h);
        ctx.lineTo(cv.width, y + h);
        ctx.stroke();
    }
    ctx.textAlign = "left";
    ctx.font = "normal  12px Arial";
    ctx.fillStyle = "black";
    ctx.fillText(hora, 4, y + 12, cv.width - 5);
}
function drawaula(id, aluno, codigo, cor, y, h, overlap, overlapx, overlapy, inicio, fim, statusfin, statusnome) {
    if (y == 0) {
        y = 1;
    }
    var cv = document.getElementById(id);
    var ctx = cv.getContext("2d");

    ctx.fillStyle = cor;
    if (overlap && overlapx != 0) {
        drawroundrect(ctx, 3, y + overlapy + 5, 142, h - overlapy / 2, 10, "#4e226c", cor);
        var img = document.getElementById("editaula");
        ctx.drawImage(img, 126, y + overlapy + 8);
    }
    else {
        drawroundrect(ctx, 3, y, 142, h, 10, "#4e226c", cor);
        var img = document.getElementById("alunoprontuario");
        ctx.drawImage(img, 93, y + 3);
        var img = document.getElementById("historicoaula");
        ctx.drawImage(img, 110, y + 3);
        var img = document.getElementById("editaula");
        ctx.drawImage(img, 128, y + 3);
    }
    ctx.textAlign = "left";
    ctx.font = "normal 12px Arial";
    ctx.fillStyle = "black";

    var gapsta = 12;

    if (overlap && overlapx != 0) {
        ctx.fillText(aluno, 6, y + overlapy + 18, 85);
        ctx.font = "normal 10px Arial";
        ctx.fillText(inicio + " - " + fim, overlapx + 4, y + overlapy + 30, 128 - overlapx);
    }
    else {
        ctx.fillText(aluno, 6, y + 15, 85);
        ctx.font = "normal 11px Arial";
        if (codigo != "") {
            codigo = "( " + codigo + " )";
            ctx.fillText(codigo, 6, y + 30, 91);
            gapsta = 50;
            if (h > 50) {
                ctx.font = "normal 10px Arial";
                ctx.fillText(statusnome, 6, y + 45, 140);
            }
        }
        else {
            if (h > 50) {
                ctx.font = "normal 10px Arial";
                gapsta = 135;
                ctx.fillText(statusnome, 6, y + 30, 140);
            }
        }

        if (statusfin == 2 || statusfin == 7) {
            ctx.beginPath();
            ctx.strokeStyle = "black";
            ctx.fillStyle = "yellow";
            ctx.arc(gapsta, y + 26, 4, 0, 2 * Math.PI);
            ctx.fill();
            ctx.stroke();
        }
        else
            if (statusfin != 3) {
                ctx.beginPath();
                ctx.strokeStyle = "black";
                ctx.fillStyle = "red";
                ctx.arc(gapsta, y + 26, 4, 0, 2 * Math.PI);
                ctx.fill();
                ctx.stroke();
            }
    }
}
function drawalunofixo(id, aluno, cor, y, h, overlap, overlapx, overlapy, inicio, fim) {
    if (y == 0) {
        y = 1;
    }
    var cv = document.getElementById(id);
    var ctx = cv.getContext("2d");

    if (overlap && overlapx != 0) {
        drawroundrect(ctx, 15, y + overlapy, 120, 30, 10, "#4e226c", cor);
    }
    else {
        drawroundrect(ctx, 15, y, 120, h, 10, "#4e226c", cor);
    }

    ctx.textAlign = "left";
    ctx.font = "normal  12px Arial";
    ctx.fillStyle = "black";
    if (overlap && overlapx != 0) {
        ctx.fillText(aluno, 20, y + overlapy + 15, 118);
        ctx.font = "normal  10px Arial";
        ctx.fillText(inicio + " - " + fim, 20, y + overlapy + 25, 100 - overlapx - 10);
    }
    else {
        ctx.fillText(aluno, 20, y + 18, 100);

    }
}
function drawaulagrupo(id, aulanome, salanome ,cor, y, h) {
    if (y == 0) {
        y = 1;
    }
    var cv = document.getElementById(id);
    var ctx = cv.getContext("2d");

    ctx.fillStyle = cor;
    drawroundrect(ctx,3, y, 125, h, 10, "#4e226c", cor);
    var img = document.getElementById("editaula");
    ctx.drawImage(img, 5, y + 3);


    ctx.textAlign = "left";
    ctx.font = "normal  12px Arial";
    ctx.fillStyle = "black";

    ctx.fillText(aulanome, 22, y + 15, 85);
    ctx.fillText(salanome, 22, y + 30, 85);

}

function drawroundrect(ctx, x, y, width, height, radius, linecor, fillcor) {
    if (width < 2 * radius) radius = width / 2;
    if (height < 2 * radius) radius = height / 2;
    ctx.beginPath();
    ctx.moveTo(x + radius, y);
    ctx.arcTo(x + width, y, x + width, y + height, radius);
    ctx.arcTo(x + width, y + height, x, y + height, radius);
    ctx.arcTo(x, y + height, x, y, radius);
    ctx.arcTo(x, y, x + width, y, radius);
    ctx.fillStyle = fillcor;
    ctx.lineWidth = 0.5;
    ctx.fill();
    ctx.strokeStyle = linecor;
    ctx.stroke();
    ctx.closePath();
}
