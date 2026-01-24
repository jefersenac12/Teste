
function salvarReserva() {
    alert('Salvar reserva (implemente integração com backend).');
    var overlay = document.getElementById('reservation-form-overlay');
    if (overlay) overlay.classList.add('d-none');
}

let mesAtual = 8;
let anoAtual = 2024;
let diaSelecionado = null;
let modoMultiSelecao = false;
let diasSelecionados = new Set();
let statusDatas = {};
function carregarStatusDatas() {
    const salvos = localStorage.getItem('statusDatasCalendario');
    if (salvos) statusDatas = JSON.parse(salvos);
    if (Object.keys(statusDatas).length === 0) {
        statusDatas = {
            '2024-09-01': 'disponivel',
            '2024-09-02': 'agencia',
            '2024-09-03': 'lotado',
            '2024-09-04': 'disponivel',
            '2024-09-06': 'lotado',
            '2024-09-07': 'disponivel',
            '2024-09-08': 'disponivel',
            '2024-09-09': 'agencia',
            '2024-09-10': 'lotado',
            '2024-09-11': 'disponivel',
            '2024-09-13': 'disponivel',
            '2024-09-14': 'disponivel',
            '2024-09-15': 'lotado',
            '2024-09-16': 'agencia',
            '2024-09-18': 'disponivel',
            '2024-09-19': 'disponivel',
            '2024-09-20': 'agencia',
            '2024-09-21': 'lotado',
            '2024-09-22': 'disponivel',
            '2024-09-24': 'disponivel',
            '2024-09-25': 'agencia',
            '2024-09-26': 'lotado',
            '2024-09-27': 'disponivel',
            '2024-09-29': 'disponivel',
            '2024-09-30': 'agencia'
        };
        salvarStatusDatas();
    }
}
function salvarStatusDatas() {
    localStorage.setItem('statusDatasCalendario', JSON.stringify(statusDatas));
}
const nomesMeses = ['Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho', 'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'];
const diasSemana = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'];
function gerarCalendario() {
    const container = document.getElementById('calendario-container');
    if (!container) return;
    const primeiroDia = new Date(anoAtual, mesAtual, 1);
    const ultimoDia = new Date(anoAtual, mesAtual + 1, 0);
    const diasNoMes = ultimoDia.getDate();
    const diaSemanaInicio = primeiroDia.getDay();
    const titulo = document.getElementById('calendario-mes-ano');
    if (titulo) titulo.textContent = `${nomesMeses[mesAtual]} ${anoAtual}`;
    let html = '<div class="row g-0">';
    diasSemana.forEach(dia => { html += `<div class="col text-center fw-semibold small text-muted py-2 border-bottom">${dia}</div>`; });
    html += '</div>';
    html += '<div class="row g-0">';
    const ultimoDiaMesAnterior = new Date(anoAtual, mesAtual, 0).getDate();
    for (let i = diaSemanaInicio - 1; i >= 0; i--) {
        const dia = ultimoDiaMesAnterior - i;
        html += `<div class="col calendar-day other-month">${dia}</div>`;
    }
    let contadorDia = diaSemanaInicio;
    for (let dia = 1; dia <= diasNoMes; dia++) {
        const dataStr = `${anoAtual}-${String(mesAtual + 1).padStart(2, '0')}-${String(dia).padStart(2, '0')}`;
        const status = statusDatas[dataStr] || 'disponivel';
        let classeSelecionado = '';
        if (modoMultiSelecao) {
            if (diasSelecionados.has(dataStr)) classeSelecionado = 'multi-selected';
        } else {
            if (diaSelecionado === dia) classeSelecionado = 'selected';
        }
        html += `<div class="col calendar-day ${classeSelecionado}" onclick="selecionarDia(${dia})" data-dia="${dia}" data-data="${dataStr}">`;
        html += `<div class="text-center">${dia}</div>`;
        html += `<span class="status-dot status-${status}"></span>`;
        html += `</div>`;
        contadorDia++;
        if (contadorDia % 7 === 0 && dia < diasNoMes) html += '</div><div class="row g-0">';
    }
    const diasRestantes = 42 - (diaSemanaInicio + diasNoMes);
    for (let dia = 1; dia <= diasRestantes; dia++) {
        html += `<div class="col calendar-day other-month">${dia}</div>`;
    }
    html += '</div>';
    container.innerHTML = html;
}
let dataParaMarcar = null;
function toggleModoMultiSelecao() {
    const chk = document.getElementById('modoMultiSelecao');
    if (!chk) return;
    modoMultiSelecao = chk.checked;
    diaSelecionado = null;
    diasSelecionados.clear();
    atualizarInfoSelecao();
    gerarCalendario();
}
function selecionarDia(dia) {
    const dataStr = `${anoAtual}-${String(mesAtual + 1).padStart(2, '0')}-${String(dia).padStart(2, '0')}`;
    if (modoMultiSelecao) {
        if (diasSelecionados.has(dataStr)) { diasSelecionados.delete(dataStr); } else { diasSelecionados.add(dataStr); }
        atualizarInfoSelecao();
        gerarCalendario();
    } else {
        diaSelecionado = dia;
        dataParaMarcar = dataStr;
        const dataFormatada = new Date(anoAtual, mesAtual, dia).toLocaleDateString('pt-BR', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
        const titulo = document.getElementById('modal-titulo-status');
        const texto = document.getElementById('data-selecionada-texto');
        const modal = document.getElementById('modal-status-dia');
        if (titulo) titulo.textContent = 'Marcar Status do Dia';
        if (texto) texto.textContent = dataFormatada;
        if (modal) modal.classList.remove('d-none');
    }
}
function atualizarInfoSelecao() {
    const contador = diasSelecionados.size;
    const infoDiv = document.getElementById('info-selecao');
    const contadorSpan = document.getElementById('contador-selecionados');
    if (!infoDiv || !contadorSpan) return;
    if (contador > 0) {
        infoDiv.classList.remove('d-none');
        contadorSpan.textContent = String(contador);
    } else {
        infoDiv.classList.add('d-none');
    }
}
function abrirModalMultiplosDias() {
    if (diasSelecionados.size === 0) { alert('Selecione pelo menos um dia!'); return; }
    const datasFormatadas = Array.from(diasSelecionados).map(dataStr => {
        const [ano, mes, dia] = dataStr.split('-');
        return new Date(parseInt(ano), parseInt(mes) - 1, parseInt(dia)).toLocaleDateString('pt-BR');
    }).join(', ');
    const titulo = document.getElementById('modal-titulo-status');
    const texto = document.getElementById('data-selecionada-texto');
    const modal = document.getElementById('modal-status-dia');
    if (titulo) titulo.textContent = `Marcar Status de ${diasSelecionados.size} Dia(s)`;
    if (texto) texto.textContent = datasFormatadas;
    if (modal) modal.classList.remove('d-none');
}
function limparSelecao() {
    diasSelecionados.clear();
    atualizarInfoSelecao();
    gerarCalendario();
}
function marcarStatus(status) {
    if (modoMultiSelecao && diasSelecionados.size > 0) {
        const quantidade = diasSelecionados.size;
        diasSelecionados.forEach(dataStr => { statusDatas[dataStr] = status; });
        salvarStatusDatas();
        diasSelecionados.clear();
        atualizarInfoSelecao();
        gerarCalendario();
        fecharModalStatus();
        alert(`${quantidade} dia(s) marcado(s) com sucesso!`);
    } else if (dataParaMarcar) {
        statusDatas[dataParaMarcar] = status;
        salvarStatusDatas();
        gerarCalendario();
        fecharModalStatus();
        alert('Status do dia atualizado com sucesso!');
    }
}
function fecharModalStatus() {
    const modal = document.getElementById('modal-status-dia');
    if (modal) modal.classList.add('d-none');
    dataParaMarcar = null;
    if (!modoMultiSelecao) diaSelecionado = null;
}
function mesAnterior() {
    mesAtual--;
    if (mesAtual < 0) { mesAtual = 11; anoAtual--; }
    diaSelecionado = null;
    diasSelecionados.clear();
    atualizarInfoSelecao();
    gerarCalendario();
}
function mesProximo() {
    mesAtual++;
    if (mesAtual > 11) { mesAtual = 0; anoAtual++; }
    diaSelecionado = null;
    diasSelecionados.clear();
    atualizarInfoSelecao();
    gerarCalendario();
}
function agendar() {
    const form = document.getElementById('configurar-agendamento-form');
    if (!form) return;
    if (!form.checkValidity()) { form.reportValidity(); return; }
    const formData = new FormData(form);
    const agendamento = {
        nomeSafra: formData.get('nomeSafra'),
        vagasTotais: formData.get('vagasTotais'),
        vagasDisponiveis: formData.get('vagasDisponiveis'),
        publicoPermitido: formData.get('publicoPermitido')
    };
    console.log('Agendar:', agendamento);
    alert('Agendamento configurado com sucesso!');
    limparFormulario();
}
function limparFormulario() {
    const form = document.getElementById('configurar-agendamento-form');
    if (!form) return;
    form.reset();
    const vt = document.getElementById('vagasTotais');
    const vd = document.getElementById('vagasDisponiveis');
    if (vt) vt.value = 30;
    if (vd) vd.value = 30;
    diaSelecionado = null;
    gerarCalendario();
}

function abrirModalAgendamento() {
    const form = document.getElementById('novo-agendamento-form');
    const id = document.getElementById('agendamentoId');
    const titulo = document.getElementById('modal-titulo');
    const overlay = document.getElementById('agendamento-form-overlay');
    if (form) form.reset();
    if (id) id.value = '';
    if (titulo) titulo.textContent = 'Novo Agendamento';
    if (overlay) overlay.classList.remove('d-none');
}
function fecharModalAgendamento() {
    const overlay = document.getElementById('agendamento-form-overlay');
    const form = document.getElementById('novo-agendamento-form');
    const id = document.getElementById('agendamentoId');
    const titulo = document.getElementById('modal-titulo');
    if (overlay) overlay.classList.add('d-none');
    if (form) form.reset();
    if (id) id.value = '';
    if (titulo) titulo.textContent = 'Novo Agendamento';
}
function editarAgendamento(id, atividade, safra, data, hora, vagasTotais, vagasDisponiveis) {
    const idEl = document.getElementById('agendamentoId');
    const atividadeEl = document.getElementById('atividade');
    const safraEl = document.getElementById('safra');
    const dataEl = document.getElementById('data');
    const horaEl = document.getElementById('hora');
    const vtEl = document.getElementById('vagasTotais');
    const vdEl = document.getElementById('vagasDisponiveis');
    const titulo = document.getElementById('modal-titulo');
    const overlay = document.getElementById('agendamento-form-overlay');
    if (idEl) idEl.value = id;
    if (atividadeEl) atividadeEl.value = atividade;
    if (safraEl) safraEl.value = safra;
    if (dataEl) dataEl.value = data;
    if (horaEl) horaEl.value = hora;
    if (vtEl) vtEl.value = vagasTotais;
    if (vdEl) vdEl.value = vagasDisponiveis;
    if (titulo) titulo.textContent = 'Editar Agendamento';
    if (overlay) overlay.classList.remove('d-none');
}
function excluirAgendamento(id, atividade) {
    if (confirm(`Deseja excluir o agendamento "${atividade}"?`)) {
        console.log('Excluir agendamento:', id);
        alert(`Agendamento "${atividade}" excluído com sucesso!`);
    }
}
function salvarAgendamento() {
    const form = document.getElementById('novo-agendamento-form');
    if (!form) return;
    if (!form.checkValidity()) { form.reportValidity(); return; }
    const formData = new FormData(form);
    const agendamentoId = formData.get('agendamentoId');
    const agendamento = {
        id: agendamentoId || null,
        atividade: formData.get('atividade'),
        safra: formData.get('safra'),
        data: formData.get('data'),
        hora: formData.get('hora'),
        vagasTotais: formData.get('vagasTotais'),
        vagasDisponiveis: formData.get('vagasDisponiveis')
    };
    console.log(agendamentoId ? 'Editar agendamento:' : 'Novo agendamento:', agendamento);
    alert(agendamentoId ? 'Agendamento atualizado com sucesso!' : 'Agendamento registrado com sucesso!');
    fecharModalAgendamento();
}

function abrirModalPagamento() {
    const form = document.getElementById('novo-pagamento-form');
    const id = document.getElementById('pagamentoId');
    const titulo = document.getElementById('modal-titulo');
    const overlay = document.getElementById('pagamento-form-overlay');
    if (form) form.reset();
    if (id) id.value = '';
    if (titulo) titulo.textContent = 'Registrar Pagamento';
    if (overlay) overlay.classList.remove('d-none');
}
function filtrarStatus(status) {
    document.querySelectorAll('.btn-sm').forEach(btn => {
        btn.classList.remove('active');
        if (btn.classList.contains('btn-secondary')) {
            btn.classList.remove('btn-secondary');
            btn.classList.add('btn-outline-secondary');
        }
    });
    if (event && event.target) {
        event.target.classList.add('active');
        if (event.target.classList.contains('btn-outline-warning') ||
            event.target.classList.contains('btn-outline-success') ||
            event.target.classList.contains('btn-outline-danger')) {
            event.target.classList.remove('btn-outline-warning', 'btn-outline-success', 'btn-outline-danger');
            event.target.classList.add('btn-secondary');
        }
    }
    console.log('Filtrar por:', status);
}
function verDetalhes(id) {
    console.log('Ver detalhes do pagamento:', id);
    alert('Detalhes do pagamento #' + id);
}
function atualizarStatus(id, status) {
    if (confirm(`Deseja atualizar o status para "${status}"?`)) {
        console.log('Atualizar status:', id, status);
        alert(`Status atualizado para "${status}" com sucesso!`);
    }
}
function visualizarTransacao(numero) {
    alert('Número de transação: ' + numero);
}
function excluirPagamento(id, idReserva) {
    if (confirm(`Deseja excluir o pagamento ${idReserva}?`)) {
        console.log('Excluir pagamento:', id);
        alert(`Pagamento ${idReserva} excluído com sucesso!`);
    }
}
function editarPagamento(id, idReserva, cliente, valor, status, data, numeroTransacao) {
    const idEl = document.getElementById('pagamentoId');
    const idReservaEl = document.getElementById('idReserva');
    const clienteEl = document.getElementById('cliente');
    const valorEl = document.getElementById('valor');
    const statusEl = document.getElementById('status');
    const dataEl = document.getElementById('data');
    const numeroEl = document.getElementById('numeroTransacao');
    const titulo = document.getElementById('modal-titulo');
    const overlay = document.getElementById('pagamento-form-overlay');
    if (idEl) idEl.value = id;
    if (idReservaEl) idReservaEl.value = idReserva;
    if (clienteEl) clienteEl.value = cliente;
    if (valorEl) valorEl.value = valor;
    if (statusEl) statusEl.value = status;
    if (dataEl) dataEl.value = data;
    if (numeroEl) numeroEl.value = numeroTransacao;
    if (titulo) titulo.textContent = 'Editar Pagamento';
    if (overlay) overlay.classList.remove('d-none');
}
function fecharModalPagamento() {
    const overlay = document.getElementById('pagamento-form-overlay');
    const form = document.getElementById('novo-pagamento-form');
    const id = document.getElementById('pagamentoId');
    const titulo = document.getElementById('modal-titulo');
    if (overlay) overlay.classList.add('d-none');
    if (form) form.reset();
    if (id) id.value = '';
    if (titulo) titulo.textContent = 'Registrar Pagamento';
}
function salvarPagamento() {
    const form = document.getElementById('novo-pagamento-form');
    if (!form) return;
    if (!form.checkValidity()) { form.reportValidity(); return; }
    const formData = new FormData(form);
    const pagamentoId = formData.get('pagamentoId');
    const pagamento = {
        id: pagamentoId || null,
        idReserva: formData.get('idReserva'),
        cliente: formData.get('cliente'),
        valor: formData.get('valor'),
        status: formData.get('status'),
        data: formData.get('data'),
        numeroTransacao: formData.get('numeroTransacao')
    };
    console.log(pagamentoId ? 'Editar pagamento:' : 'Novo pagamento:', pagamento);
    alert(pagamentoId ? 'Pagamento atualizado com sucesso!' : 'Pagamento registrado com sucesso!');
    fecharModalPagamento();
}

function toggleCamposPorTipo() {
    const tipo = document.getElementById('tipo');
    const campoTelefone = document.getElementById('campo-telefone');
    const campoCnpj = document.getElementById('campo-cnpj');
    const telefoneInput = document.getElementById('telefone');
    const cnpjInput = document.getElementById('cnpj');
    if (!tipo || !campoTelefone || !campoCnpj || !telefoneInput || !cnpjInput) return;
    const v = tipo.value;
    if (v === 'Familia') {
        campoTelefone.classList.remove('d-none');
        telefoneInput.setAttribute('required', 'required');
        campoCnpj.classList.add('d-none');
        cnpjInput.removeAttribute('required');
    } else if (v === 'Agencia') {
        campoTelefone.classList.add('d-none');
        telefoneInput.removeAttribute('required');
        campoCnpj.classList.remove('d-none');
        cnpjInput.setAttribute('required', 'required');
    } else {
        campoTelefone.classList.remove('d-none');
        campoCnpj.classList.add('d-none');
        telefoneInput.setAttribute('required', 'required');
        cnpjInput.removeAttribute('required');
    }
}
function aplicarMascaraTelefone(value) {
    return value.replace(/\D/g, '').replace(/(\d{2})(\d)/, '($1) $2').replace(/(\d{5})(\d)/, '$1-$2').replace(/(-\d{4})\d+?$/, '$1');
}
function aplicarMascaraCNPJ(value) {
    return value.replace(/\D/g, '').replace(/(\d{2})(\d)/, '$1.$2').replace(/(\d{3})(\d)/, '$1.$2').replace(/(\d{3})(\d)/, '$1/$2').replace(/(\d{4})(\d)/, '$1-$2').replace(/(-\d{2})\d+?$/, '$1');
}
function salvarUsuario() {
    const form = document.getElementById('novo-usuario-form');
    if (!form) return;
    if (!form.checkValidity()) { form.reportValidity(); return; }
    const formData = new FormData(form);
    const usuario = {
        nome: formData.get('nome'),
        tipo: formData.get('tipo'),
        telefone: formData.get('telefone'),
        cnpj: formData.get('cnpj'),
        email: formData.get('email'),
        senha: formData.get('senha')
    };
    console.log('Novo usuário:', usuario);
    alert('Usuário salvo com sucesso!');
    const overlay = document.getElementById('user-form-overlay');
    if (overlay) overlay.classList.add('d-none');
    form.reset();
}

document.addEventListener('DOMContentLoaded', function () {
    const tel = document.getElementById('telefone');
    if (tel) tel.addEventListener('input', function (e) { e.target.value = aplicarMascaraTelefone(e.target.value); });
    const cnpj = document.getElementById('cnpj');
    if (cnpj) cnpj.addEventListener('input', function (e) { e.target.value = aplicarMascaraCNPJ(e.target.value); });
    const tipo = document.getElementById('tipo');
    if (tipo) toggleCamposPorTipo();
    const cal = document.getElementById('calendario-container');
    if (cal) { carregarStatusDatas(); gerarCalendario(); }
});

function salvarAtividade() {
    alert('Salvar atividade (implemente integração com backend).');
    var overlay = document.getElementById('atividade-form-overlay');
    if (overlay) overlay.classList.add('d-none');
}

