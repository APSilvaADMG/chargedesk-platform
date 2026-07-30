// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: SPA Platform — paridade (ticket, painel vivo, caixa) + estacionamento.

const Api = {
  async req(method, path, body) {
    const opt = { method, headers: { 'Content-Type': 'application/json' } };
    if (body !== undefined) opt.body = JSON.stringify(body);
    const res = await fetch('/api' + path, opt);
    const txt = await res.text();
    let data = null;
    try { data = txt ? JSON.parse(txt) : null; } catch { data = txt; }
    if (!res.ok) throw new Error(typeof data === 'string' ? data : (data?.title || data?.detail || txt || res.statusText));
    return data;
  },
  get: (p) => Api.req('GET', p),
  post: (p, b) => Api.req('POST', p, b)
};

const statusLabel = (s) => ({ 1:'Criado',5:'Em execução',6:'Aguardando pag.',7:'Finalizado',8:'Cancelado' }[s] || s);
const tipoLabel = (t) => ({ 1:'Carga', 2:'Estacion.' }[t] || t);
const moeda = (v) => (v ?? 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
const hora = (d) => d ? new Date(d).toLocaleString('pt-BR') : '—';
const formaLabel = (f) => ({ 1:'PIX',2:'Dinheiro',3:'Débito',4:'Crédito',5:'Cortesia' }[f] || f);

let session = JSON.parse(localStorage.getItem('cdp.session') || 'null');
let modal;
let painelTimer = null;

function showApp() {
  document.getElementById('loginView').classList.add('d-none');
  document.getElementById('appView').classList.remove('d-none');
  document.getElementById('userLabel').textContent = session?.nome || '';
  navigate('dashboard');
}

function showLogin() {
  stopPainelLive();
  document.getElementById('appView').classList.add('d-none');
  document.getElementById('loginView').classList.remove('d-none');
}

function stopPainelLive() {
  if (painelTimer) { clearInterval(painelTimer); painelTimer = null; }
}

async function navigate(page) {
  stopPainelLive();
  document.querySelectorAll('.nav-link-btn').forEach(b => b.classList.toggle('active', b.dataset.page === page));
  const el = document.getElementById('page');
  el.innerHTML = '<div class="text-muted">Carregando…</div>';
  try {
    if (page === 'dashboard') await renderDashboard(el);
    else if (page === 'caixa') await renderCaixa(el);
    else if (page === 'atendimentos') await renderAtendimentos(el);
    else if (page === 'estacionamento') await renderEstacionamento(el);
    else if (page === 'clientes') await renderClientes(el);
    else if (page === 'veiculos') await renderVeiculos(el);
  } catch (e) {
    el.innerHTML = `<div class="alert alert-danger">${e.message}</div>`;
  }
}

async function renderDashboard(el) {
  const fill = async () => {
    const [caixa, atend, dispCarga, dispVaga] = await Promise.all([
      Api.get('/caixa/atual'),
      Api.get('/atendimentos'),
      Api.get('/equipamentos/disponiveis?tipo=1'),
      Api.get('/equipamentos/disponiveis?tipo=10')
    ]);
    const ativos = (atend || []).filter(a => a.statusAtendimento === 5).length;
    const pend = (atend || []).filter(a => a.statusAtendimento === 6).length;
    const host = document.getElementById('dashBody') || el;
    host.innerHTML = `
      <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
        <h2 class="h4 mb-0">Painel <span class="badge text-bg-secondary fw-normal" id="liveBadge">ao vivo</span></h2>
        <div class="d-flex gap-2">
          <button class="btn btn-success" id="btnNova">+ Nova carga</button>
          <button class="btn btn-outline-success" id="btnNovaEst">+ Entrada pátio</button>
        </div>
      </div>
      <div class="row g-3 mb-3">
        <div class="col-md-2"><div class="stat"><div class="text-muted small">Caixa</div><strong>${caixa ? 'Aberto #' + caixa.numero : 'Fechado'}</strong></div></div>
        <div class="col-md-2"><div class="stat"><div class="text-muted small">Em execução</div><strong>${ativos}</strong></div></div>
        <div class="col-md-2"><div class="stat"><div class="text-muted small">Aguardando pag.</div><strong>${pend}</strong></div></div>
        <div class="col-md-3"><div class="stat"><div class="text-muted small">Pontos livres</div><strong>${(dispCarga||[]).length}</strong></div></div>
        <div class="col-md-3"><div class="stat"><div class="text-muted small">Vagas livres</div><strong>${(dispVaga||[]).length}</strong></div></div>
      </div>
      <div class="table-card">
        <h6>Últimos atendimentos</h6>
        ${tabelaAtendimentos((atend||[]).slice(0, 12))}
      </div>`;
    document.getElementById('btnNova').onclick = abrirNovaCarga;
    document.getElementById('btnNovaEst').onclick = abrirEntradaPatio;
  };

  el.innerHTML = '<div id="dashBody"></div>';
  await fill();
  painelTimer = setInterval(() => {
    if (!document.getElementById('dashBody')) { stopPainelLive(); return; }
    fill().catch(() => {});
  }, 15000);
}

function tabelaAtendimentos(lista) {
  if (!lista.length) return '<p class="text-muted mb-0">Nenhum atendimento.</p>';
  return `<div class="table-responsive"><table class="table table-sm align-middle mb-0">
    <thead><tr><th>Ticket</th><th>Tipo</th><th>Cliente</th><th>Placa</th><th>Início</th><th>Tempo</th><th>Valor</th><th>Status</th><th></th></tr></thead>
    <tbody>${lista.map(a => `<tr>
      <td>#${a.ticket ?? '—'}</td>
      <td>${tipoLabel(a.tipo)}</td>
      <td>${a.clienteNome || '—'}</td>
      <td>${a.placa || '—'}</td>
      <td>${hora(a.abertoEm)}</td>
      <td>${a.tempo || '—'}</td>
      <td>${a.valor != null ? moeda(a.valor) : '—'}</td>
      <td><span class="badge badge-st-${a.statusAtendimento}">${statusLabel(a.statusAtendimento)}</span></td>
      <td class="text-end text-nowrap">${acoesAtendimento(a)}</td>
    </tr>`).join('')}</tbody></table></div>`;
}

function acoesAtendimento(a) {
  const parts = [];
  parts.push(`<button class="btn btn-sm btn-outline-secondary" onclick="imprimirTicket('${a.id}')" title="Ticket">🎫</button>`);
  if (a.statusAtendimento === 5)
    parts.push(`<button class="btn btn-sm btn-outline-primary" onclick="finalizarAt('${a.id}')">Finalizar</button>`);
  if (a.statusAtendimento === 6)
    parts.push(`<button class="btn btn-sm btn-success" onclick="pagarAt('${a.id}')">Pagar</button>`);
  return parts.join(' ');
}

async function renderAtendimentos(el) {
  const lista = await Api.get('/atendimentos');
  el.innerHTML = `
    <div class="d-flex justify-content-between mb-3 flex-wrap gap-2">
      <h2 class="h4 mb-0">Atendimentos</h2>
      <div class="d-flex gap-2">
        <button class="btn btn-success" id="btnNova2">+ Nova carga</button>
        <button class="btn btn-outline-success" id="btnNovaEst2">+ Entrada pátio</button>
      </div>
    </div>
    <div class="table-card">${tabelaAtendimentos(lista || [])}</div>`;
  document.getElementById('btnNova2').onclick = abrirNovaCarga;
  document.getElementById('btnNovaEst2').onclick = abrirEntradaPatio;
}

async function renderEstacionamento(el) {
  const [vagasLivres, atend, mapa] = await Promise.all([
    Api.get('/equipamentos/disponiveis?tipo=10'),
    Api.get('/atendimentos?status=5'),
    Api.get('/equipamentos?tipo=10')
  ]);
  const ativosEst = (atend || []).filter(a => a.tipo === 2);
  el.innerHTML = `
    <div class="d-flex justify-content-between mb-3">
      <h2 class="h4 mb-0">Estacionamento</h2>
      <button class="btn btn-success" id="btnEnt">+ Entrada</button>
    </div>
    <div class="row g-3 mb-3">
      <div class="col-md-4"><div class="stat"><div class="text-muted small">Vagas livres</div><strong>${(vagasLivres||[]).length} / ${(mapa||[]).length}</strong></div></div>
      <div class="col-md-4"><div class="stat"><div class="text-muted small">Ocupadas agora</div><strong>${ativosEst.length}</strong></div></div>
    </div>
    <div class="table-card mb-3">
      <h6>Mapa de vagas</h6>
      <div class="d-flex flex-wrap gap-2">${(mapa||[]).map(v => {
        const livre = (vagasLivres||[]).some(l => l.id === v.id);
        return `<span class="badge ${livre ? 'text-bg-success' : 'text-bg-danger'} p-2">${v.nome} · ${livre ? 'livre' : 'ocupada'}</span>`;
      }).join('') || '<span class="text-muted">Sem vagas cadastradas</span>'}</div>
    </div>
    <div class="table-card">
      <h6>Em execução (pátio)</h6>
      ${tabelaAtendimentos(ativosEst)}
    </div>`;
  document.getElementById('btnEnt').onclick = abrirEntradaPatio;
}

async function renderCaixa(el) {
  const [caixa, hist] = await Promise.all([
    Api.get('/caixa/atual'),
    Api.get('/caixa/historico')
  ]);
  let recebHtml = '';
  if (caixa) {
    const recs = await Api.get(`/caixa/${caixa.id}/recebimentos`);
    recebHtml = `
      <h6 class="mt-4">Recebimentos deste caixa</h6>
      ${(recs||[]).length ? `<table class="table table-sm"><thead><tr><th>Hora</th><th>Ticket</th><th>Forma</th><th>Valor</th></tr></thead>
        <tbody>${recs.map(r => `<tr>
          <td>${hora(r.dataHora)}</td>
          <td>#${r.ticket ?? '—'}</td>
          <td>${formaLabel(r.forma)}</td>
          <td>${moeda(r.valor)}</td>
        </tr>`).join('')}</tbody></table>` : '<p class="text-muted">Nenhum recebimento ainda.</p>'}`;
  }
  el.innerHTML = `
    <h2 class="h4 mb-3">Caixa</h2>
    <div class="table-card mb-3">
      ${caixa ? `
        <p><strong>Caixa #${caixa.numero}</strong> aberto em ${hora(caixa.dataAbertura)}</p>
        <p>Valor inicial: ${moeda(caixa.valorInicial)}</p>
        <button class="btn btn-danger" id="btnFechar">Fechar caixa</button>
        ${recebHtml}
      ` : `
        <p class="text-muted">Nenhum caixa aberto.</p>
        <div class="row g-2 align-items-end">
          <div class="col-auto"><label class="form-label">Valor inicial</label><input type="number" step="0.01" class="form-control" id="valorIni" value="100"></div>
          <div class="col-auto"><button class="btn btn-primary" id="btnAbrir">Abrir caixa</button></div>
        </div>
      `}
    </div>
    <div class="table-card">
      <h6>Histórico (últimos 30)</h6>
      <table class="table table-sm mb-0"><thead><tr><th>#</th><th>Abertura</th><th>Fechamento</th><th>Inicial</th><th>Informado</th><th>Status</th></tr></thead>
      <tbody>${(hist||[]).map(c => `<tr>
        <td>${c.numero}</td><td>${hora(c.dataAbertura)}</td><td>${hora(c.dataFechamento)}</td>
        <td>${moeda(c.valorInicial)}</td><td>${c.valorInformado != null ? moeda(c.valorInformado) : '—'}</td>
        <td>${c.statusCaixa === 1 ? 'Aberto' : 'Fechado'}</td>
      </tr>`).join('') || '<tr><td colspan="6" class="text-muted">Vazio</td></tr>'}</tbody></table>
    </div>`;
  const btnAbrir = document.getElementById('btnAbrir');
  if (btnAbrir) btnAbrir.onclick = async () => {
    try {
      await Api.post('/caixa/abrir', {
        empresaId: session.empresaId,
        unidadeId: session.unidadeId,
        operadorId: session.id,
        valorInicial: +document.getElementById('valorIni').value || 0
      });
      navigate('caixa');
    } catch (e) { alert(e.message); }
  };
  const btnFechar = document.getElementById('btnFechar');
  if (btnFechar) btnFechar.onclick = async () => {
    const valor = prompt('Valor informado no fechamento:', '0');
    if (valor == null) return;
    try {
      await Api.post(`/caixa/${caixa.id}/fechar`, { valorInformado: +valor || 0 });
      navigate('caixa');
    } catch (e) { alert(e.message); }
  };
}

async function renderClientes(el) {
  const lista = await Api.get('/clientes');
  el.innerHTML = `
    <div class="d-flex justify-content-between mb-3">
      <h2 class="h4 mb-0">Clientes</h2>
      <button class="btn btn-primary" id="btnNovoCli">+ Cliente</button>
    </div>
    <div class="table-card"><table class="table table-sm"><thead><tr><th>Nome</th><th>Telefone</th><th>E-mail</th></tr></thead>
    <tbody>${(lista||[]).map(c => `<tr><td>${c.nome}</td><td>${c.telefone||'—'}</td><td>${c.email||'—'}</td></tr>`).join('') || '<tr><td colspan="3" class="text-muted">Nenhum</td></tr>'}
    </tbody></table></div>`;
  document.getElementById('btnNovoCli').onclick = () => {
    modalTitle.textContent = 'Novo cliente';
    modalBody.innerHTML = `
      <input class="form-control mb-2" id="cNome" placeholder="Nome *">
      <input class="form-control mb-2" id="cTel" placeholder="Telefone *">
      <input class="form-control" id="cEmail" placeholder="E-mail">`;
    modalFooter.innerHTML = `<button class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
      <button class="btn btn-primary" id="btnSaveC">Salvar</button>`;
    btnSaveC.onclick = async () => {
      try {
        await Api.post('/clientes', {
          empresaId: session.empresaId,
          nome: cNome.value,
          telefone: cTel.value,
          email: cEmail.value || null
        });
        modal.hide();
        navigate('clientes');
      } catch (e) { alert(e.message); }
    };
    modal.show();
  };
}

async function renderVeiculos(el) {
  const lista = await Api.get('/veiculos');
  el.innerHTML = `
    <div class="d-flex justify-content-between mb-3">
      <h2 class="h4 mb-0">Veículos</h2>
      <button class="btn btn-primary" id="btnNovoV">+ Veículo</button>
    </div>
    <div class="table-card"><table class="table table-sm"><thead><tr><th>Placa</th><th>Cliente</th><th>Marca/Modelo</th><th>Conector</th></tr></thead>
    <tbody>${(lista||[]).map(v => `<tr><td>${v.placa}</td><td>${v.clienteNome}</td><td>${[v.marca,v.modelo].filter(Boolean).join(' ')||'—'}</td><td>${v.conector||'—'}</td></tr>`).join('') || '<tr><td colspan="4" class="text-muted">Nenhum</td></tr>'}
    </tbody></table></div>`;
  document.getElementById('btnNovoV').onclick = async () => {
    const clientes = await Api.get('/clientes');
    if (!clientes?.length) { alert('Cadastre um cliente antes.'); return; }
    modalTitle.textContent = 'Novo veículo';
    modalBody.innerHTML = `
      <select class="form-select mb-2" id="vCli">${clientes.map(c=>`<option value="${c.id}">${c.nome}</option>`).join('')}</select>
      <input class="form-control mb-2" id="vPlaca" placeholder="Placa *">
      <input class="form-control mb-2" id="vMarca" placeholder="Marca">
      <input class="form-control mb-2" id="vModelo" placeholder="Modelo">
      <input class="form-control" id="vCon" placeholder="Conector (CCS2…)">`;
    modalFooter.innerHTML = `<button class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
      <button class="btn btn-primary" id="btnSaveV">Salvar</button>`;
    btnSaveV.onclick = async () => {
      try {
        await Api.post('/veiculos', {
          empresaId: session.empresaId,
          clienteId: vCli.value,
          placa: vPlaca.value,
          marca: vMarca.value || null,
          modelo: vModelo.value || null,
          conector: vCon.value || null
        });
        modal.hide();
        navigate('veiculos');
      } catch (e) { alert(e.message); }
    };
    modal.show();
  };
}

async function abrirNovaCarga() {
  try {
    const caixa = await Api.get('/caixa/atual');
    if (!caixa) { alert('Abra o caixa antes de iniciar uma carga.'); navigate('caixa'); return; }
  } catch { alert('Não foi possível verificar o caixa.'); return; }

  const [veiculos, pontos, prox] = await Promise.all([
    Api.get('/veiculos'),
    Api.get('/equipamentos/disponiveis?tipo=1'),
    Api.get('/atendimentos/proximo-ticket')
  ]);
  if (!veiculos?.length) { alert('Cadastre um veículo antes.'); navigate('veiculos'); return; }
  if (!pontos?.length) { alert('Não há pontos de carregamento disponíveis no momento.'); return; }

  const rotulo = (v) => {
    const mm = [v.marca, v.modelo].filter(Boolean).join(' ');
    return mm ? `${v.placa} — ${mm}` : v.placa;
  };

  modalTitle.textContent = 'Nova carga';
  modalBody.innerHTML = `
    <div class="mb-2"><label class="form-label">Ticket</label>
      <input type="number" class="form-control" id="fTicket" value="${prox.ticket||''}"></div>
    <div class="mb-2"><label class="form-label">Veículo *</label>
      <select class="form-select" id="fVeic">${veiculos.map(v=>`<option value="${v.id}">${rotulo(v)}</option>`).join('')}</select></div>
    <div class="mb-2"><label class="form-label">Cliente</label>
      <input class="form-control" id="fCliNome" readonly>
      <input type="hidden" id="fCliId"></div>
    <div class="mb-2"><label class="form-label">Telefone</label>
      <input class="form-control" id="fCliTel" readonly></div>
    <div class="mb-2"><label class="form-label">Ponto disponível *</label>
      <select class="form-select" id="fPonto">${pontos.map(p=>`<option value="${p.id}">${p.nome}</option>`).join('')}</select></div>`;
  const sync = () => {
    const v = veiculos.find(x => x.id === fVeic.value);
    fCliId.value = v?.clienteId || '';
    fCliNome.value = v?.clienteNome || '';
    fCliTel.value = v?.clienteTelefone || '—';
  };
  fVeic.onchange = sync; sync();
  modalFooter.innerHTML = `<button class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
    <button class="btn btn-success" id="btnIni">Iniciar carregamento</button>`;
  btnIni.onclick = async () => {
    try {
      const criado = await Api.post('/atendimentos/carregamento', {
        empresaId: session.empresaId,
        unidadeId: session.unidadeId,
        operadorId: session.id,
        veiculoId: fVeic.value,
        equipamentoId: fPonto.value,
        ticket: +fTicket.value || 0
      });
      modal.hide();
      if (criado?.id) window.open(`/api/atendimentos/${criado.id}/ticket`, '_blank');
      navigate('atendimentos');
    } catch (e) { alert(e.message); }
  };
  modal.show();
}

async function abrirEntradaPatio() {
  try {
    const caixa = await Api.get('/caixa/atual');
    if (!caixa) { alert('Abra o caixa antes de registrar entrada.'); navigate('caixa'); return; }
  } catch { alert('Não foi possível verificar o caixa.'); return; }

  const [veiculos, vagas, prox] = await Promise.all([
    Api.get('/veiculos'),
    Api.get('/equipamentos/disponiveis?tipo=10'),
    Api.get('/atendimentos/proximo-ticket')
  ]);
  if (!veiculos?.length) { alert('Cadastre um veículo antes.'); navigate('veiculos'); return; }
  if (!vagas?.length) { alert('Não há vagas disponíveis no momento.'); return; }

  const rotulo = (v) => {
    const mm = [v.marca, v.modelo].filter(Boolean).join(' ');
    return mm ? `${v.placa} — ${mm}` : v.placa;
  };

  modalTitle.textContent = 'Entrada — Estacionamento';
  modalBody.innerHTML = `
    <div class="mb-2"><label class="form-label">Ticket</label>
      <input type="number" class="form-control" id="eTicket" value="${prox.ticket||''}"></div>
    <div class="mb-2"><label class="form-label">Veículo *</label>
      <select class="form-select" id="eVeic">${veiculos.map(v=>`<option value="${v.id}">${rotulo(v)}</option>`).join('')}</select></div>
    <div class="mb-2"><label class="form-label">Cliente</label>
      <input class="form-control" id="eCliNome" readonly></div>
    <div class="mb-2"><label class="form-label">Telefone</label>
      <input class="form-control" id="eCliTel" readonly></div>
    <div class="mb-2"><label class="form-label">Vaga disponível *</label>
      <select class="form-select" id="eVaga">${vagas.map(p=>`<option value="${p.id}">${p.nome}</option>`).join('')}</select></div>`;
  const sync = () => {
    const v = veiculos.find(x => x.id === eVeic.value);
    eCliNome.value = v?.clienteNome || '';
    eCliTel.value = v?.clienteTelefone || '—';
  };
  eVeic.onchange = sync; sync();
  modalFooter.innerHTML = `<button class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
    <button class="btn btn-success" id="btnEntOk">Registrar entrada</button>`;
  btnEntOk.onclick = async () => {
    try {
      const criado = await Api.post('/atendimentos/estacionamento', {
        empresaId: session.empresaId,
        unidadeId: session.unidadeId,
        operadorId: session.id,
        veiculoId: eVeic.value,
        equipamentoId: eVaga.value,
        ticket: +eTicket.value || 0
      });
      modal.hide();
      if (criado?.id) window.open(`/api/atendimentos/${criado.id}/ticket`, '_blank');
      navigate('estacionamento');
    } catch (e) { alert(e.message); }
  };
  modal.show();
}

window.imprimirTicket = (id) => {
  window.open(`/api/atendimentos/${id}/ticket`, '_blank');
};

window.finalizarAt = async (id) => {
  if (!confirm('Finalizar este atendimento?')) return;
  try {
    await Api.post(`/atendimentos/${id}/finalizar`, {});
    navigate(document.querySelector('.nav-link-btn.active')?.dataset.page || 'atendimentos');
  } catch (e) { alert(e.message); }
};

window.pagarAt = async (id) => {
  const det = await Api.get(`/atendimentos/${id}`);
  modalTitle.textContent = `Pagar ticket #${det.ticket}`;
  modalBody.innerHTML = `
    <p>${det.clienteNome} · ${det.placa || ''} · <strong>${moeda(det.valor)}</strong></p>
    <p class="small text-muted">${tipoLabel(det.tipo)}${det.equipamentoNome ? ' · ' + det.equipamentoNome : ''}${det.vaga ? ' · ' + det.vaga : ''}</p>
    <select class="form-select mb-2" id="fForma">
      <option value="1">PIX</option><option value="2">Dinheiro</option>
      <option value="3">Débito</option><option value="4">Crédito</option><option value="5">Cortesia</option>
    </select>
    <input class="form-control" id="fMotivo" placeholder="Motivo (obrigatório se cortesia)">`;
  modalFooter.innerHTML = `<button class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
    <button class="btn btn-success" id="btnPag">Confirmar pagamento</button>`;
  btnPag.onclick = async () => {
    try {
      await Api.post(`/atendimentos/${id}/pagar`, {
        forma: +fForma.value,
        motivoCortesia: fMotivo.value || null
      });
      modal.hide();
      navigate('atendimentos');
    } catch (e) { alert(e.message); }
  };
  modal.show();
};

document.getElementById('btnLogin').onclick = async () => {
  loginErr.textContent = '';
  try {
    const u = await Api.post('/auth/login', {
      login: loginUser.value,
      senha: loginPass.value
    });
    session = u;
    localStorage.setItem('cdp.session', JSON.stringify(u));
    showApp();
  } catch (e) {
    loginErr.textContent = e.message || 'Falha no login';
  }
};

document.getElementById('btnLogout').onclick = () => {
  session = null;
  localStorage.removeItem('cdp.session');
  showLogin();
};

document.querySelectorAll('.nav-link-btn').forEach(b => {
  b.onclick = () => navigate(b.dataset.page);
});

document.addEventListener('DOMContentLoaded', () => {
  modal = new bootstrap.Modal('#modal');
  if (session?.id) showApp();
  else showLogin();
});
