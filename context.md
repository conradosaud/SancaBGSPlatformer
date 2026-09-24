# Contexto do Projeto: 3D Platformer Express (BGS Demo)

## 1. Visão Geral & Escopo
* **Evento:** Demonstração em stand de cursos na Brasil Game Show (BGS).
* **Prazo de Desenvolvimento:** 8 horas (escopo estritamente travado).
* **Tempo Médio de Gameplay:** 4 a 5 minutos por sessão (alta rotatividade).
* **Público & Plataforma:** Jogadores casuais a entusiastas no evento; foco exclusivo em **Gamepad/Controle** (Xbox/PlayStation).
* **Diretriz de Design:** Sem combate, sem inteligência artificial, sem telas de carregamento intermediárias. Foco total em plataforma 3D vertical, responsividade de controle, fluidez de movimentação e resolução intuitiva de travessia.

---

## 2. Pilares Técnicos & Stack
* **Engine:** Unity 6.000.6.0f1
* **Template Base:** *Starter Assets - Third Person Controller* oficial da Unity.
  * **Sistema de Movimento:** `CharacterController` cinemático padrão (nunca substitua por `Rigidbody`).
  * **Câmera:** Cinemachine Virtual Camera com follow em 3ª pessoa e rotação travada no analógico direito.
  * **Input:** *New Input System* da Unity com mapeamento padronizado para controles.
* **Cenário:** Pacote modular pré-existente (estruturado em blocos e plataformas de alto contraste visual).

---

## 3. Especificação das Mecânicas & Upgrades

O jogo possui uma estrutura linear onde o jogador desbloqueia habilidades que destravam o próximo trecho do cenário vertical.

### 3.1. Movimentação Base (Inicial)
* **Controles:**
  * Analógico Esquerdo: Movimentação horizontal relativa à câmera.
  * Analógico Direito: Rotação da câmera Cinemachine.
  * Botão Sul (A / Cross): Pulo simples.
* **Comportamento:** Pulo responsivo, gravidade padrão rápida para evitar sensação de "personagem flutuante".
  * Pulo estritamente vertical ou com controle direcional aéreo reduzido.

### 3.2. Gancho / Grapple (Upgrade 1)
* **Descrição:** O jogador atira um projétil que, ao atingir determinadas plataformas (tags), é puxado para ela.
* **Objetivo:** Travessia horizontal sobre grandes vãos e fendas.

### 3.3. Super Pulo (Upgrade 2)
* **Descrição:** O jogador carrega um pulo mais forte que é obrigatóriamente na vertical, impedindo de movimentar até seu término.
* **Objetivo:** Superar barreiras verticais intransponíveis pelo pulo comum.

### 3.4. Planar / Glide (Upgrade 3)
* **Descrição:** O jogador plana no ar controlando o pouso em plataformas distantes e precisas.
* **Objetivo:** Descer abismos controlando o pouso em plataformas distantes e precisas.


## 4. Estrutura da Fase (Level Design - 5 Minutos)

O percurso é uma subida contínua em espiral ou estrutura vertical:

1. **Setor 01 (Minuto 0 a 1):** Plataformas estáticas baixas para adaptação aos analógicos. O jogador se depara com um muro alto. Coleta o orbe do **Gancho**.
3. **Setor 03 (Minuto 1 a 2):** Travessia de gancho conectando 2 ou 3 orbes suspensos no ar. Chegada a um ponto alto que exige descer sobre plataformas isoladas. Coleta o orbe de **Super Pulo**.
2. **Setor 02 (Minuto 2 a 3):** Uso obrigatório do Super Pulo para alcançar o patamar superior. Encontra um grande vão sem chão. Coleta o orbe do **Planar**.
4. **Setor 04 (Minuto 3 a 4.5) — O Desafio Integrado:** Subida final combinando as mecânicas:
   * Carregar Super Pulo na base para ganhar altitude.
   * Conectar o gancho horizontal no ápice do salto.
   * Soltar o gancho e planar suavemente até a plataforma da relíquia/vitória.
5. **Setor 05 (Final):** Plataforma do cume com a Relíquia. Efeito de partículas de vitória, branding do stand/curso visível e contagem regressiva de 5 segundos para reiniciar a cena.

---

## 5. Sistemas de Suporte Essenciais

### 5.1. Sistema de Morte & Respawn Instantâneo
* **Dead Zone:** Trigger box gigantesco cobrindo toda a base abaixo do cenário.
* **Sem Game Over:** Ao tocar na zona de morte, o personagem é reposicionado imediatamente no último checkpoint ativo.
* **Tempo de transição:** Máximo de 0.2 segundos (fade rápido, sem recarregamento de cena).

### 5.2. Sistema de Checkpoints
* Triggers invisíveis ao longo da rota que atualizam uma variável `Vector3 lastCheckpointPosition`.
* Posicionados imediatamente antes de cada seção de salto difícil.

### 5.3. Loop do Estande (Kiosk Mode)
* Ausência total de menus complexos ou telas de pausa.
* Timer de inatividade (opcional, 30 segundos sem input reseta para o início).
* Tela final de "Parabéns / Curso X" com reinício automático da cena após 5 segundos.

---

## 6. Diretrizes de Código & Implementação
* **Tudo simples:** este é um projeto simples e de escopo pequeno que não tem o objetivo de crescer. Mantenha códigos e estruturas simples.
* **Não refatorar o ThirdPersonController do zero:** Prefira estender o script original da Unity ou criar um componente desacoplado `PlayerAbilities.cs` que acesse os campos do controlador de forma limpa.
* **Manter flags booleanas de progresso simples:**
  ```csharp
  public bool hasSuperJump = false;
  public bool hasGrapple = false;
  public bool hasGlide = false;
  ```
* **Prioridade Zero Bugs sobre Features:** Se alguma mecânica apresentar comportamento instável nas primeiras 4 horas, reduza seu alcance (ex: transforme o gancho em um teletransporte direto com efeito visual ou remova a mecânica mantendo as outras duas).